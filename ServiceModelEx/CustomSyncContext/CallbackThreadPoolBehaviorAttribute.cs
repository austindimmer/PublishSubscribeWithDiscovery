// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;


namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class CallbackThreadPoolBehaviorAttribute : ThreadPoolBehaviorAttribute,IEndpointBehavior
   {
      public CallbackThreadPoolBehaviorAttribute(uint poolSize,Type clientType) : this(poolSize,clientType,null)
      {}
      public CallbackThreadPoolBehaviorAttribute(uint poolSize,Type clientType,string poolName) : base(poolSize,clientType,poolName)
      {
         AppDomain.CurrentDomain.ProcessExit += delegate
                                                {
                                                   ThreadPoolHelper.CloseThreads(ServiceType);
                                                };
      }
      void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint,BindingParameterCollection bindingParameters)
      {}

      void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint,ClientRuntime clientRuntime)
      {
         IContractBehavior contractBehavior = this;
         contractBehavior.ApplyDispatchBehavior(null,serviceEndpoint,clientRuntime.CallbackDispatchRuntime);
      }

      void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint,EndpointDispatcher endpointDispatcher)
      {}

      void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
      {}
   }
}