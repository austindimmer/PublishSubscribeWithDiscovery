// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceModelEx
{
   public static class ContextManager
   {
      public const string InstanceIdKey = "instanceId";

      public static Guid InstanceId
      {
         get
         {
            string id = GetContext(InstanceIdKey) ?? Guid.Empty.ToString();
            return new Guid(id); 
         }
      }

      public static IDictionary<string,string> CreateContext(string key,string value)
      {
         IDictionary<string,string> context = new Dictionary<string,string>();
         context[key] = value;
         return context;
      }

      //Called by the service to read incoming context over context bindings
      public static string GetContext(string key)
      {
         if(OperationContext.Current == null)
         {
            return null;
         }
         if(OperationContext.Current.IncomingMessageProperties.ContainsKey(ContextMessageProperty.Name))
         {
            ContextMessageProperty contextProperty = OperationContext.Current.IncomingMessageProperties[ContextMessageProperty.Name] as ContextMessageProperty;
            if(contextProperty.Context.ContainsKey(key) == false)
            {
               return null;
            }
            return contextProperty.Context[key]; 
         }
         else
         {
            return null;
         }
      }
      
      
      //Called by the client to write context over context bindings
      public static void SetContext(IClientChannel innerChannel,string key,string value)
      {
         SetContext(innerChannel,UpdateContext(innerChannel,key,value));
      }
      
      //Called by the client to overwrite context with context bindings
      public static void SetContext(IClientChannel innerChannel,IDictionary<string,string> context)
      {
         Debug.Assert((innerChannel as ICommunicationObject).State != CommunicationState.Opened);

         IContextManager contextManager = innerChannel.GetProperty<IContextManager>();
         contextManager.SetContext(context);
      }

      //Creates new context containing the new value and the old ones from the proxy
      public static IDictionary<string,string> UpdateContext(IClientChannel innerChannel,string key,string value)
      {
         IContextManager contextManager = innerChannel.GetProperty<IContextManager>();

         IDictionary<string,string> context = new Dictionary<string,string>(contextManager.GetContext());
         context[key] = value;
         return context;
      }

      public static Guid GetInstanceId(IClientChannel innerChannel)
      {
         try
         {
            string instanceId = innerChannel.GetProperty<IContextManager>().GetContext()[InstanceIdKey];
            return new Guid(instanceId);
         }
         catch(KeyNotFoundException)
         {
            return Guid.Empty;
         }
      }

      public static void SetInstanceId(IClientChannel innerChannel,Guid instanceId)
      {
         SetContext(innerChannel,InstanceIdKey,instanceId.ToString());
      }

      //Proxy extensions
      public static void SetContext<T>(this ClientBase<T> proxy,string key,string value) where T : class
      {
         SetContext(proxy.InnerChannel,key,value);
      }

      public static void SetContext<T>(this ClientBase<T> proxy,IDictionary<string,string> context) where T : class
      {
         SetContext(proxy.InnerChannel,context);
      }

      public static IDictionary<string,string> UpdateContext<T>(this ClientBase<T> proxy,string key,string value) where T : class
      {
         return UpdateContext(proxy.InnerChannel,key,value);
      }      
      public static void SaveInstanceId(Guid instanceId,string fileName)
      {
         using(Stream stream = new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.Write))
         {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream,instanceId);
         }
      }
      public static Guid LoadInstanceId(string fileName)
      {
         try
         {
            using(Stream stream = new FileStream(fileName,FileMode.Open,FileAccess.Read))
            {
               IFormatter formatter = new BinaryFormatter();
               return (Guid)formatter.Deserialize(stream);
            }
         }
         catch
         {
            return Guid.Empty;
         }
      }
   }
}
