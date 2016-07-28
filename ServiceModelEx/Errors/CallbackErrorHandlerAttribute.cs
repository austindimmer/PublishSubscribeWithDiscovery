// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   public class CallbackErrorHandlerBehaviorAttribute : ErrorHandlerBehaviorAttribute,IEndpointBehavior
   {
      public CallbackErrorHandlerBehaviorAttribute(Type clientType)
      {
         ServiceType = clientType;
      }
      
      void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint,BindingParameterCollection bindingParameters)
      {}
      void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint,ClientRuntime behavior)
      {
         behavior.CallbackDispatchRuntime.ChannelDispatcher.ErrorHandlers.Add(this);
      }
      void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint,EndpointDispatcher dispatcher)
      {}
      void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
      {}
   }
} 





