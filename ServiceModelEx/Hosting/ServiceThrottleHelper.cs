// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ServiceModelEx
{
   public static class ServiceThrottleHelper
   {
      ///<summary>
      ///  Can only call before openning the host
      ///</summary>
      public static void SetThrottle(this ServiceHost host,int maxCalls,int maxSessions,int maxInstances)
      {
         ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
         throttle.MaxConcurrentCalls = maxCalls;
         throttle.MaxConcurrentSessions = maxSessions;
         throttle.MaxConcurrentInstances = maxInstances;
         host.SetThrottle(throttle);
      }
      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      /// <param name="serviceThrottle"></param>
      /// <param name="overrideConfig"></param>
      public static void SetThrottle(this ServiceHost host,ServiceThrottlingBehavior serviceThrottle,bool overrideConfig)
      {
         if(host.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }
         ServiceThrottlingBehavior throttle = host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
         if(throttle == null)
         {
            host.Description.Behaviors.Add(serviceThrottle);
            return; 
         }
         if(overrideConfig == false)
         {
            return;
         }
         host.Description.Behaviors.Remove(throttle);
         host.Description.Behaviors.Add(serviceThrottle);
      }
      /// <summary>
      /// Can only call before openning the host. Does not override config values if present 
      /// </summary>
      public static void SetThrottle(this ServiceHost host,ServiceThrottlingBehavior serviceThrottle)
      {
         host.SetThrottle(serviceThrottle,false);
      }
   }
}
