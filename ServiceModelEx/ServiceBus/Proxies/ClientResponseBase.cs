// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
using System.ServiceModel;
using ServiceModelEx;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;

namespace ServiceModelEx.ServiceBus
{
   public abstract class ClientResponseBase<T> : QueuedServiceBusClient<T> where T : class
   {
      public readonly string ReplyToSessionId;

      public readonly Uri ResponseAddress;

      public string LastCorrolationId
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get;
         
         [MethodImpl(MethodImplOptions.Synchronized)]
         protected set;
      }


      public ClientResponseBase(Uri responseAddress,string sessionId = null) : base(sessionId)
      {
         ResponseAddress = responseAddress;
      }
      public ClientResponseBase(Uri responseAddress,string endpointName,string sessionId) : base(endpointName,sessionId)
      {
         ResponseAddress = responseAddress;
      }
      public ClientResponseBase(Uri responseAddress,string endpointName,string sessionId,string replyToSessionId) : base(endpointName,sessionId)
      {
         ResponseAddress = responseAddress;
         ReplyToSessionId = replyToSessionId;
      }

      public ClientResponseBase(Uri responseAddress,NetMessagingBinding binding,EndpointAddress address,string sessionId = null) : base(binding,address,sessionId) 
      {
         ResponseAddress = responseAddress;
      }
      public ClientResponseBase(Uri responseAddress,NetMessagingBinding binding,EndpointAddress address,string sessionId,string replyToSessionId) : base(binding,address,sessionId) 
      {
         ResponseAddress = responseAddress;
         ReplyToSessionId = replyToSessionId;
      }
      protected override void PreInvoke(ref Message request)
      {        
         BrokeredMessageProperty property = GetMessageProperty(ref request);
         Debug.Assert(property != null);

         property.CorrelationId = GenerateMethodId();
         property.ReplyTo = ResponseAddress.AbsoluteUri;
         property.ReplyToSessionId = ReplyToSessionId;

         LastCorrolationId = property.CorrelationId;

         base.PreInvoke(ref request);
      }
      protected virtual string GenerateMethodId()
      {
         return Guid.NewGuid().ToString();
      }
   }
}
