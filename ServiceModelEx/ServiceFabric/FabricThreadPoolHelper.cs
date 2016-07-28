// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel.Description;
using System.Threading;

namespace ServiceModelEx.ServiceFabric
{
   public static class FabricThreadPoolHelper
   {
      public static void SetThrottle(this ServiceDescription serviceDescription)
      {
         //To improve performance, system moves services onto .NET worker thread pool. Set throttle to match max threads in .NET worker thread pool.
         int maxWorkerThreads,maxIocpThreads;
         ThreadPool.GetMaxThreads(out maxWorkerThreads,out maxIocpThreads);
         ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
         throttle.MaxConcurrentInstances = maxWorkerThreads;
         throttle.MaxConcurrentCalls = maxWorkerThreads;
         throttle.MaxConcurrentSessions = maxWorkerThreads;
         if(serviceDescription.Behaviors.Find<ServiceThrottlingBehavior>() == null)
         {
            serviceDescription.Behaviors.Add(throttle);
         }
      }
      public static void ConfigureThreadPool()
      {
         int minWorkerThreads,maxWorkerThreads,minIocpThreads,maxIocpThreads;
         ThreadPool.GetMaxThreads(out maxWorkerThreads,out maxIocpThreads);
         ThreadPool.GetMinThreads(out minWorkerThreads,out minIocpThreads);
         ThreadPool.SetMinThreads(maxWorkerThreads,minIocpThreads);
      }
      /// <summary>
      /// To avoid new thread creation latency, as per MSDN article KB2538826 move WCF off of IOCP thread pool.
      /// </summary>
      public static SynchronizationContext GetSynchronizer()
      {
         return new SynchronizationContext();
      }
   }
}
