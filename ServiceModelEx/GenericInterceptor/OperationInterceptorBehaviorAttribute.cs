// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Method)]
   public abstract class OperationInterceptorBehaviorAttribute : Attribute,IOperationBehavior
   {
      protected abstract GenericInvoker CreateInvoker(IOperationInvoker oldInvoker);

      public void AddBindingParameters(OperationDescription operationDescription,BindingParameterCollection bindingParameters)
      {}

      public void ApplyClientBehavior(OperationDescription operationDescription,ClientOperation clientOperation)
      {}

      public void ApplyDispatchBehavior(OperationDescription operationDescription,DispatchOperation dispatchOperation)
      {
         IOperationInvoker oldInvoker = dispatchOperation.Invoker;
         dispatchOperation.Invoker = CreateInvoker(oldInvoker);
      }

      public void Validate(OperationDescription operationDescription)
      {}
   }
}