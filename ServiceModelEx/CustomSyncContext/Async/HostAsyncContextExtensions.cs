// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace ServiceModelEx
{
   public static class HostAsyncContextExtensions 
   {
      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public static void SetAsyncContext(this ServiceHost host,uint poolSize,string threadName)
      {
         if(host.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }

         Debug.Assert(SynchronizationContext.Current == null);//Can call only once

         AsyncContextSynchronizer asyncContextSynchronizer = new AsyncContextSynchronizer(poolSize,threadName);
         SynchronizationContext.SetSynchronizationContext(asyncContextSynchronizer);

         host.Closing += delegate
                         {
                            using(asyncContextSynchronizer)
                            {}
                         };
      }
      /// <summary>
      ///  Can only call before openning the host
      /// </summary>
      public static void SetAsyncContext(this ServiceHost host,uint poolSize)
      {
         SetAsyncContext(host,poolSize,"Executing all endpoints of " + host.Description.ServiceType);
      }           
   }
}





