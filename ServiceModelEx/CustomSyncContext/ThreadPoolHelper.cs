// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   public static class ThreadPoolHelper 
   {
      static Dictionary<Type,ThreadPoolSynchronizer> m_Synchronizers = new Dictionary<Type,ThreadPoolSynchronizer>();
      
      [MethodImpl(MethodImplOptions.Synchronized)]
      internal static bool HasSynchronizer(Type type)
      {
         return m_Synchronizers.ContainsKey(type);
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      internal static ThreadPoolSynchronizer GetSynchronizer(Type type)
      {
         Debug.Assert(HasSynchronizer(type));
         return m_Synchronizers[type];
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      internal static void ApplyDispatchBehavior(ThreadPoolSynchronizer synchronizer,uint poolSize,Type type,string poolName,DispatchRuntime dispatch)
      {
         Debug.Assert(dispatch.SynchronizationContext == null);
         int maxConcurrentCalls = 16;
         if(dispatch.ChannelDispatcher.ServiceThrottle != null)
         {
            maxConcurrentCalls = dispatch.ChannelDispatcher.ServiceThrottle.MaxConcurrentCalls;
         }
         if(maxConcurrentCalls < poolSize)
         {
            throw new InvalidOperationException("The throttle should allow at least as many concurrent calls as the pool size");
         }

         if(HasSynchronizer(type))
         {
            Debug.Assert(GetSynchronizer(type) == synchronizer);
         }

         if(HasSynchronizer(type) == false)
         {
            m_Synchronizers[type] = synchronizer;
         }
         dispatch.SynchronizationContext = synchronizer;
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public static void CloseThreads(Type type)
      {
         if(HasSynchronizer(type))
         {
            m_Synchronizers[type].Dispose();
            m_Synchronizers.Remove(type);
         }     
      }
   }
}