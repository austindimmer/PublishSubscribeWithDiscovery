// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx.ServiceFabric.Services
{
   internal class ProxyMessageInterceptor : IEndpointBehavior,IClientMessageInspector
   {
      public ProxyMessageInterceptor()
      {}

      public void AddBindingParameters(ServiceEndpoint endpoint,BindingParameterCollection bindingParameters)
      {}
      public void ApplyClientBehavior(ServiceEndpoint endpoint,ClientRuntime clientRuntime)
      {
         clientRuntime.ClientMessageInspectors.Add(this);
      }
      public void ApplyDispatchBehavior(ServiceEndpoint endpoint,EndpointDispatcher endpointDispatcher)
      {}
      public void Validate(ServiceEndpoint endpoint)
      {}

      public void AfterReceiveReply(ref Message reply,object correlationState)
      {}
      public object BeforeSendRequest(ref Message request,IClientChannel channel)
      {
         return null;
      }
   }
}