// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class CallbackThreadAffinityBehaviorAttribute : CallbackThreadPoolBehaviorAttribute
   {
      public CallbackThreadAffinityBehaviorAttribute(Type clientType) : this(clientType,"Callback Worker Thread")
      {}
      public CallbackThreadAffinityBehaviorAttribute(Type clientType,string threadName) : base(1,clientType,threadName)
      {}
   }
}