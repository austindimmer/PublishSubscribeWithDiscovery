// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx.ServiceFabric
{
   internal class FabricThreadPoolBehavior : IEndpointBehavior
   {
      public void AddBindingParameters(ServiceEndpoint serviceEndpoint,BindingParameterCollection bindingParameters)
      {}
      public virtual void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint,EndpointDispatcher endpointDispatcher)
      {
         endpointDispatcher.DispatchRuntime.SynchronizationContext = FabricThreadPoolHelper.GetSynchronizer();
      }
      public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint,ClientRuntime behavior)
      {}
      public void Validate(ServiceEndpoint endpoint) 
      {}
   }
}
