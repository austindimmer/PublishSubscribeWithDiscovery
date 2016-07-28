// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceModelEx.ServiceBus
{
   public abstract class QueuedServiceBusClient<T> : InterceptorClientBase<T>,IServiceBusProperties where T : class
   {
      public string SessionId
      {get;protected set;}

      /// <summary>
      /// sessionId should be null for no session
      /// </summary>
      /// <param name="sessionId"></param>
      public QueuedServiceBusClient(string sessionId = null) 
      {
         SessionId = sessionId;
      }
      public QueuedServiceBusClient(string endpointName,string sessionId) : base(endpointName)
      {
         SessionId = sessionId;
      }

      public QueuedServiceBusClient(NetMessagingBinding binding,EndpointAddress address,string sessionId = null) : base(binding,address) 
      {
         SessionId = sessionId;
      }

      protected override T CreateChannel()
      {
         Debug.Assert(Endpoint.Binding is NetMessagingBinding);

         bool requiresSession;

         if(SessionId == null)
         {
            requiresSession = false;
         }
         else
         {
            requiresSession = true;
         }
         IServiceBusProperties properties = this as IServiceBusProperties;
         Tuple<Uri,string> tuple = ServiceBusHelper.ParseUri(Endpoint.Address.Uri);
         ServiceBusHelper.VerifyQueue(tuple.Item1,tuple.Item2,properties.Credential.TokenProvider,requiresSession);

         this.AddGenericResolver();
         return base.CreateChannel();
      }

      protected BrokeredMessageProperty GetMessageProperty(ref Message request)
      {
         BrokeredMessageProperty property;

         if(request.Properties.ContainsKey(BrokeredMessageProperty.Name) == false)
         {
            property = new BrokeredMessageProperty();
            request.Properties.Add(BrokeredMessageProperty.Name,property);
         }
         else
         {
            property = request.Properties[BrokeredMessageProperty.Name] as BrokeredMessageProperty;
         }
         return property;
      }
      protected override void PreInvoke(ref Message request)
      {
         BrokeredMessageProperty property = GetMessageProperty(ref request);
         Debug.Assert(property != null);

         property.SessionId = SessionId;

         base.PreInvoke(ref request);
      }
      protected TransportClientEndpointBehavior Credential
      {
         get
         {
            IServiceBusProperties properties = this;
            return properties.Credential;
         }
         set
         {
            IServiceBusProperties properties = this;
            properties.Credential = value;
         }
      }

      TransportClientEndpointBehavior IServiceBusProperties.Credential
      {
         get
         {
            if(Endpoint.Behaviors.Contains(typeof(TransportClientEndpointBehavior)))
            {
               return Endpoint.Behaviors.Find<TransportClientEndpointBehavior>();
            }
            else
            {
               TransportClientEndpointBehavior credential = new TransportClientEndpointBehavior();
               Credential = credential;
               return Credential;
            }
         }
         set
         {
            Debug.Assert(Endpoint.Behaviors.Contains(typeof(TransportClientEndpointBehavior)) == false);
            Endpoint.Behaviors.Add(value);
         }
      }

      Uri[] IServiceBusProperties.Addresses
      {
         get
         {
            return new Uri[]{Endpoint.Address.Uri};
         }
      }
   }
}




 
