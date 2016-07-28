// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx.ServiceFabric.Actors
{
   public class ActorOperationBehavior : IOperationBehavior
   {
      public void AddBindingParameters(OperationDescription operationDescription,BindingParameterCollection bindingParameters)
      {}
      public void ApplyClientBehavior(OperationDescription operationDescription,ClientOperation clientOperation)
      {}
      public void ApplyDispatchBehavior(OperationDescription operationDescription,DispatchOperation dispatchOperation)
      {
         dispatchOperation.Invoker = new ActorOperationInvoker(dispatchOperation.Invoker,operationDescription);
      }
      public void Validate(OperationDescription operationDescription)
      {}
   }
}
