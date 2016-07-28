// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx.ServiceFabric.Actors
{
   internal class ProxyMessageInterceptor : IEndpointBehavior,IClientMessageInspector
   {
      ActorId m_ActorId
      {get;set;}

      public ProxyMessageInterceptor(ActorId actorId)
      {
         m_ActorId = actorId;
      }

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
         GenericContext<ActorId> context = new GenericContext<ActorId>(m_ActorId);
         MessageHeader<GenericContext<ActorId>> genericHeader = new MessageHeader<GenericContext<ActorId>>(context);
         request.Headers.Add(genericHeader.GetUntypedHeader(GenericContext<ActorId>.TypeName,GenericContext<ActorId>.TypeNamespace));
         return null;
      }
   }
}