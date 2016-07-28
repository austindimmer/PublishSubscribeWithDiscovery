// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.ServiceBus.Messaging;
using ServiceModelEx.ServiceBus;
using System.ServiceModel.Channels;

namespace ServiceModelEx.ServiceBus
{
   public abstract class ServiceQueueResponseBase<T> : QueuedServiceBusClient<T> where T : class 
   {
      public ServiceQueueResponseBase() : this(Binding)
      {}
      public ServiceQueueResponseBase(string bindingName) : this(new NetMessagingBinding(bindingName))
      {}
      
      public ServiceQueueResponseBase(NetMessagingBinding binding) : base(binding,ResponseAddress)                                                                
      {
         //Grab the creds the host was using 
         IServiceBusProperties properties = OperationContext.Current.Host as IServiceBusProperties;
         Debug.Assert(properties != null);

         Tuple<Uri,string> tuple = ServiceBusHelper.ParseUri(ResponseAddress.Uri);

         this.SetServiceBusCredentials(properties.Credential.TokenProvider);

         ServiceBusHelper.VerifyQueue(tuple.Item1,tuple.Item2,properties.Credential.TokenProvider,RequiresSession);
      }

      protected override void PreInvoke(ref Message request)
      {
         BrokeredMessageProperty property = GetMessageProperty(ref request);
         Debug.Assert(property != null);

         BrokeredMessageProperty contextProperty = OperationContext.Current.IncomingMessageProperties[BrokeredMessageProperty.Name] as BrokeredMessageProperty;
         SessionId = contextProperty.SessionId;

         property.CorrelationId = contextProperty.CorrelationId;
         property.ReplyToSessionId = contextProperty.ReplyToSessionId;

         base.PreInvoke(ref request);
      }


      static EndpointAddress ResponseAddress
      {
         get
         {
            BrokeredMessageProperty property = OperationContext.Current.IncomingMessageProperties[BrokeredMessageProperty.Name] as BrokeredMessageProperty;
            return new EndpointAddress(property.ReplyTo);
         }
      }
      static NetMessagingBinding Binding
      {
         get
         {
            return OperationContext.Current.Host.Description.Endpoints[0].Binding as NetMessagingBinding;
         }
      }
      static bool RequiresSession
      {
         get
         {
            BrokeredMessageProperty property = OperationContext.Current.IncomingMessageProperties[BrokeredMessageProperty.Name] as BrokeredMessageProperty;
            return property.ReplyToSessionId != null;

         }
      }
   }
}





