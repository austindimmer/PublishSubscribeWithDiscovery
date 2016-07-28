// ©2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   public abstract class InterceptorChannelFactory<T> : ChannelFactory<T> where T : class
   {
      public InterceptorChannelFactory() 
      {
         Endpoint.Behaviors.Add(new ClientInterceptor(this));
      }
      public InterceptorChannelFactory(string endpointName) : base(endpointName)
      {
         Endpoint.Behaviors.Add(new ClientInterceptor(this));
      }
      public InterceptorChannelFactory(Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         Endpoint.Behaviors.Add(new ClientInterceptor(this));
      }
      protected virtual void PreInvoke(ref Message request)
      {}

      protected virtual void PostInvoke(ref Message reply)
      {}

      class ClientInterceptor : IEndpointBehavior,IClientMessageInspector 
      {
         InterceptorChannelFactory<T> Proxy
         {get;set;}

         internal ClientInterceptor(InterceptorChannelFactory<T> proxy)
         {
            Proxy = proxy;
         }
         object IClientMessageInspector.BeforeSendRequest(ref Message request,IClientChannel channel)
         {
            Proxy.PreInvoke(ref request);
            return null;
         }
         void IClientMessageInspector.AfterReceiveReply(ref Message reply,object correlationState)
         {
            Proxy.PostInvoke(ref reply);
         }

         void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint,ClientRuntime clientRuntime)
         {
            clientRuntime.MessageInspectors.Add(this);
         }

         void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint,BindingParameterCollection bindingParameters)
         {}
         void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint,EndpointDispatcher endpointDispatcher)
         {}
         void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
         {}
      }      
   }
}