// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class ThreadAffinityBehaviorAttribute : ThreadPoolBehaviorAttribute
   {
      public ThreadAffinityBehaviorAttribute(Type serviceType) : this(serviceType,"Affinity Worker Thread")
      {}
      public ThreadAffinityBehaviorAttribute(Type serviceType,string threadName) : base(1,serviceType,threadName)
      {}
   }
}