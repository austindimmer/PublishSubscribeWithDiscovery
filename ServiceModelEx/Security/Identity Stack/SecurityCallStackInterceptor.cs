// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel.Dispatcher;


namespace ServiceModelEx
{
   class SecurityCallStackInterceptor : GenericInvoker
   {
      public SecurityCallStackInterceptor(IOperationInvoker oldInvoker) : base(oldInvoker)
      {}

      protected override void PreInvoke(object instance,object[] inputs)
      {
         try
         {
            SecurityCallStack callStack = SecurityCallStackContext.Current;
            if(callStack != null)
            {
               LogCallChain(callStack);
               ValidateCallChain(callStack);
               SignCallChain(callStack);
            }
         }
         catch(NullReferenceException)
         {
            throw new InvalidOperationException("No security call stack was found. Are you using the right proxy?");
         }
      }

      void ValidateCallChain(SecurityCallStack callStack)
      {
         //Perform custom validation steps here
      }

      void SignCallChain(SecurityCallStack callStack)
      {
         //Digitally sign call stack here
      }
      void LogCallChain(SecurityCallStack callStack)
      {
         //Log call stack here. For example:
         foreach(SecurityCallFrame call in callStack.Calls)
         {
            Trace.Write("Activity ID = " + call.ActivityId + ",");
            Trace.Write(" Address = " + call.Address + ",");
            Trace.Write(" Authentication = " + call.Authentication + ",");
            Trace.Write(" Time = " + call.CallTime + ",");
            Trace.Write(" Identity = " + call.IdentityName + ",");
            Trace.Write(" Operation = " + call.Operation + ",");
            Trace.WriteLine(" Caller = " + call.CallerType);
         }
      }
   }

   public class OperationSecurityCallStackAttribute : OperationInterceptorBehaviorAttribute
   {
      protected override GenericInvoker CreateInvoker(IOperationInvoker oldInvoker)
      {
         return new SecurityCallStackInterceptor(oldInvoker);
      }
   }
   public class SecurityCallStackBehaviorAttribute : ServiceInterceptorBehaviorAttribute
   {
      protected override OperationInterceptorBehaviorAttribute CreateOperationInterceptor()
      {
         return new OperationSecurityCallStackAttribute();
      }
   }
}