// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class AsyncContextBehaviorAttribute : ThreadPoolBehaviorAttribute
   {
      public AsyncContextBehaviorAttribute(uint poolSize,Type serviceType) : this(poolSize,serviceType,"Async Context Pool Thread: ")
      {}
      public AsyncContextBehaviorAttribute(uint poolSize,Type serviceType,string threadName) : base(poolSize,serviceType,threadName)
      {}
      protected override ThreadPoolSynchronizer ProvideSynchronizer()
      {
         if(ThreadPoolHelper.HasSynchronizer(ServiceType) == false)
         {
            return new AsyncContextSynchronizer(PoolSize,PoolName);
         }
         else
         {
            return ThreadPoolHelper.GetSynchronizer(ServiceType);
         }
      }
   }
}