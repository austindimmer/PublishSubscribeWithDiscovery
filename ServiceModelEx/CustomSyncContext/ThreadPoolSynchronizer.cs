// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;

namespace ServiceModelEx
{
   [SecurityPermission(SecurityAction.Demand,ControlThread = true)]
   public class ThreadPoolSynchronizer : SynchronizationContext,IDisposable
   {
      protected class WorkerThread
      {
         ThreadPoolSynchronizer m_Context;
         public Thread m_ThreadObj;
         bool m_EndLoop;

         public int ManagedThreadId
         {
            get
            {
               return m_ThreadObj.ManagedThreadId;
            }
         }

         internal WorkerThread(string name,ThreadPoolSynchronizer context)
         {
            m_Context = context;

            m_EndLoop = false;
            m_ThreadObj = null;

            m_ThreadObj = new Thread(Run);
            m_ThreadObj.IsBackground = true;
            m_ThreadObj.Name = name;
            m_ThreadObj.Start();
         }
         bool EndLoop
         {
            set
            {
               lock(this)
               {
                  m_EndLoop = value;
               }
            }
            get
            {
               lock(this)
               {
                  return m_EndLoop;
               }
            }
         }
         void Start()
         {
            Debug.Assert(m_ThreadObj != null);
            Debug.Assert(m_ThreadObj.IsAlive == false);
            m_ThreadObj.Start();
         }

         internal virtual void ProcessItem(WorkItem workItem)
         {
            workItem.CallBack();
         }
         void Run()
         {
            Debug.Assert(SynchronizationContext.Current == null);
            SynchronizationContext.SetSynchronizationContext(m_Context);

            while(EndLoop == false)
            {
               WorkItem workItem = m_Context.GetNext();
               if(workItem != null)
               {
                  ProcessItem(workItem);
               }
            }
         }
         public void Kill()
         {
            //Kill is called on client thread - must use cached thread object
            Debug.Assert(m_ThreadObj != null);
            if(m_ThreadObj.IsAlive == false)
            {
               return;
            }
            EndLoop = true;

            //Wait for thread to die
            m_ThreadObj.Join();
         }
      }

      protected WorkerThread[] m_WorkerThreads;
      Queue<WorkItem> m_WorkItemQueue;
      protected Semaphore CallQueued
      {get;private set;}

      protected virtual void InitializeThreads(uint poolSize,string poolName)
      {
         m_WorkerThreads = new WorkerThread[poolSize];
         for(int index = 0;index<poolSize;index++)
         {
            m_WorkerThreads[index] = new WorkerThread(poolName + " " + (index+1),this);
         }
      }

      public ThreadPoolSynchronizer(uint poolSize) : this(poolSize,"Pooled Thread: ")
      {}
      public ThreadPoolSynchronizer(uint poolSize,string poolName)
      {
         if(poolSize == 0)
         {
            throw new InvalidOperationException("Pool size cannot be zero");
         }
         CallQueued = new Semaphore(0,Int32.MaxValue);
         m_WorkItemQueue = new Queue<WorkItem>();

         InitializeThreads(poolSize,poolName);
      }
      virtual internal void QueueWorkItem(WorkItem workItem)
      {
         lock(m_WorkItemQueue)
         {
            m_WorkItemQueue.Enqueue(workItem);
            CallQueued.Release();
         }
      }
      protected virtual bool QueueEmpty
      {
         get
         {
            lock(m_WorkItemQueue)
            {
               if(m_WorkItemQueue.Count > 0)
               {
                  return false;
               }
               return true;
            }
         }
      }
      internal virtual WorkItem GetNext()
      {
         CallQueued.WaitOne();
         lock(m_WorkItemQueue)
         {
            if(m_WorkItemQueue.Count == 0)
            {
               return null;
            }
            return m_WorkItemQueue.Dequeue();
         }
      }
      public void Dispose()
      {
         Close();
      }
      public override SynchronizationContext CreateCopy()
      {
         return this;
      }
      public override void Post(SendOrPostCallback method,object state)
      {
         WorkItem workItem = new WorkItem(method,state);
         QueueWorkItem(workItem);
      }
      public override void Send(SendOrPostCallback method,object state)
      {
         //If already on the correct context, must invoke now to avoid deadlock
         if(SynchronizationContext.Current == this)
         {
            method(state);
            return;
         }
         WorkItem workItem = new WorkItem(method,state);
         QueueWorkItem(workItem);
         workItem.AsyncWaitHandle.WaitOne();
      }
      public void Close()
      {
         if(CallQueued.SafeWaitHandle.IsClosed)
         {
            return;
         }
         CallQueued.Release(Int32.MaxValue);

         foreach(WorkerThread thread in m_WorkerThreads)
         {
            thread.Kill();
         }
         CallQueued.Close();
      }
      public void Abort()
      {
         CallQueued.Release(Int32.MaxValue);

         foreach(WorkerThread thread in m_WorkerThreads)
         {
            thread.m_ThreadObj.Abort();
         }
         CallQueued.Close();
      }
   }
}