// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;


namespace ServiceModelEx
{
   public abstract class ContextClientBase<T> : ClientBase<T> where T : class
   {
      public Guid InstanceId
      {
         get
         {
            return ContextManager.GetInstanceId(InnerChannel);
         }
      }

      public ContextClientBase()
      {}

      public ContextClientBase(string endpointName) : base(endpointName)
      {}

      public ContextClientBase(Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {}

      public ContextClientBase(Guid instanceId) : this(ContextManager.InstanceIdKey,instanceId.ToString())
      {}

      public ContextClientBase(Guid instanceId,string endpointName) : this(ContextManager.InstanceIdKey,instanceId.ToString(),endpointName)
      {}

      public ContextClientBase(Guid instanceId,Binding binding,EndpointAddress remoteAddress) : this(ContextManager.InstanceIdKey,instanceId.ToString(),binding,remoteAddress)
      {}

      public ContextClientBase(string key,string value) : this(ContextManager.CreateContext(key,value))
      {}

      public ContextClientBase(string key,string value,string endpointName) : this(ContextManager.CreateContext(key,value),endpointName)
      {}

      public ContextClientBase(string key,string value,Binding binding,EndpointAddress remoteAddress) : this(ContextManager.CreateContext(key,value),binding,remoteAddress)
      {}
      
      public ContextClientBase(IDictionary<string,string> context)
      {
         SetContext(context);
      }

      public ContextClientBase(IDictionary<string,string> context,string endpointName) : base(endpointName)
      {
         SetContext(context);
      }

      public ContextClientBase(IDictionary<string,string> context,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         SetContext(context);
      }

      public T DuplicateProxy()
      {
         T channel = ChannelFactory<T>.CreateChannel(Endpoint.Binding,Endpoint.Address);
         IClientChannel innerChannel = channel as IClientChannel;
         
         Guid instanceId = ContextManager.GetInstanceId(InnerChannel);
         ContextManager.SetInstanceId(innerChannel,instanceId);
         return channel;
      }
      void SetContext(IDictionary<string,string> context)
      {
         VerifyContextBinding();

         //Special case is context with only instance ID that is the empty guid
         if(context.Count == 1)
         {
            if(context.ContainsKey(ContextManager.InstanceIdKey))
            {
               if(context[ContextManager.InstanceIdKey] == Guid.Empty.ToString())
               {
                  return;
               }
            }
         }
         ContextManager.SetContext(InnerChannel,context);
      }

      void VerifyContextBinding()
      {
         BindingElementCollection elements = Endpoint.Binding.CreateBindingElements();
         
         if(elements.Contains(typeof(ContextBindingElement)))
         {
            return;
         }
         throw new InvalidOperationException("Can only use a context binding with " + GetType());
      }
   }
}