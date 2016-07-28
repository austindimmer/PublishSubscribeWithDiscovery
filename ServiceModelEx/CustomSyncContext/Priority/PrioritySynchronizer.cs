// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Security.Permissions;
using System.ServiceModel;

namespace ServiceModelEx
{
   public enum CallPriority
   {
      Low,
      Normal,
      High
   }
   [SecurityPermission(SecurityAction.Demand,ControlThread = true)]
   public class PrioritySynchronizer : ThreadPoolSynchronizer
   {
      const string SlotName = "CallPriority";
      Queue<WorkItem> m_LowPriorityItemQueue;
      Queue<WorkItem> m_NormalPriorityItemQueue;
      Queue<WorkItem> m_HighPriorityItemQueue;

      public static CallPriority Priority
      {
         get
         {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(SlotName);
            object data = Thread.GetData(slot);
            if(data == null)
            {
               return CallPriority.Normal;
            }
            return (CallPriority)data;
         }
         set
         {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(SlotName);
            Thread.SetData(slot,value);
         }
      }
      public PrioritySynchronizer(uint poolSize) : this(poolSize,"Pooled Thread: ")
      {}

      public PrioritySynchronizer(uint poolSize,string poolName) : base(poolSize,poolName)
      {
         m_LowPriorityItemQueue     = new Queue<WorkItem>();
         m_NormalPriorityItemQueue  = new Queue<WorkItem>();
         m_HighPriorityItemQueue    = new Queue<WorkItem>();
      }
      internal override void QueueWorkItem(WorkItem workItem)
      {
         CallPriority priority;

         GenericContext<CallPriority> context = GenericContext<CallPriority>.Current;
         if(context == null)
         {
            priority = Priority;//Read from TLS
         }
         else
         {
            priority = context.Value;
         }
         switch(priority)
         {
            case CallPriority.Low:
            {
               lock(m_LowPriorityItemQueue)
               {
                  m_LowPriorityItemQueue.Enqueue(workItem);
                  CallQueued.Release();
                  return;
               }
            }
            case CallPriority.Normal:
            {
               lock(m_NormalPriorityItemQueue)
               {
                  m_NormalPriorityItemQueue.Enqueue(workItem);
                  CallQueued.Release();
                  return;
               }
            }
            case CallPriority.High:
            {
               lock(m_HighPriorityItemQueue)
               {
                  m_HighPriorityItemQueue.Enqueue(workItem);
                  CallQueued.Release();
                  return;
               }
            }
            default:
            {
               throw new InvalidOperationException("Unknown priority value: " + priority);
            }
         }
      }
      internal override WorkItem GetNext()
      {
         CallQueued.WaitOne();
         lock(m_HighPriorityItemQueue)
         {
            if(m_HighPriorityItemQueue.Count >= 1)
            {
               return m_HighPriorityItemQueue.Dequeue();
            }
         }
         lock(m_NormalPriorityItemQueue)
         {
            if(m_NormalPriorityItemQueue.Count >= 1)
            {
               return m_NormalPriorityItemQueue.Dequeue();
            }
         }
         lock(m_LowPriorityItemQueue)
         {
            if(m_LowPriorityItemQueue.Count >= 1)
            {
               return m_LowPriorityItemQueue.Dequeue();
            }
         }
         return null;
      }
      protected override bool QueueEmpty
      {
         get
         {
            lock(m_LowPriorityItemQueue)
            {
               if(m_LowPriorityItemQueue.Count > 0)
               {
                  return false;
               }
            }
            lock(m_NormalPriorityItemQueue)
            {
               if(m_NormalPriorityItemQueue.Count > 0)
               {
                  return false;
               }
            }
            lock(m_HighPriorityItemQueue)
            {
               if(m_HighPriorityItemQueue.Count > 0)
               {
                  return false;
               }
            }
            return true;
         }
      }
   }
}