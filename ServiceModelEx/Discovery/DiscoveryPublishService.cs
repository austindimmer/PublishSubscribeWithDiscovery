// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;

namespace ServiceModelEx
{
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,UseSynchronizationContext = false)]
   public abstract class DiscoveryPublishService<T> : IDisposable where T : class
   {
      protected internal AnnouncementSink<T>   AnnouncedSubscribers
      {get;set;}

      protected internal DiscoveredServices<T> DiscoveredServices
      {get;set;}

      public readonly static Uri Scope;

      static DiscoveryPublishService()
      {
         //Strip all special characters that are invalid URL characters
         string typeName = typeof(T).ToString();
         typeName = typeName.Replace("[","");
         typeName = typeName.Replace("]","");
         typeName = typeName.Replace("`","");

         Scope = new Uri("net.tcp://ServiceModelEx.DiscoveryPublishService." + typeName);
      }
      static NetTcpBinding Binding
      {
         get
         {
            return new NetTcpBinding(SecurityMode.Transport,true);
         }
      }

      public static T CreateChannel()
      {
         EndpointAddress address = DiscoveryHelper.DiscoverAddress<T>(Scope);
         return ChannelFactory<T>.CreateChannel(Binding,address);
      }

      public static ServiceHost<S> CreateHost<S>() where S : DiscoveryPublishService<T>,T
      {
         ServiceHost<S> host = DiscoveryFactory.CreateDiscoverableHost<S>(Scope,false);

         foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
         {
            if(endpoint.Address.Uri.Scheme == Uri.UriSchemeNetTcp)
            {
               endpoint.Binding = Binding;
            }
         }
         return host;
      }

      public DiscoveryPublishService()
      {
         AnnouncedSubscribers = new AnnouncementSink<T>();    
         DiscoveredServices = new DiscoveredServices<T>();

         AnnouncedSubscribers.Open();
         DiscoveredServices.Open();
      }
      public void Dispose()
      {
         AnnouncedSubscribers.Close();
         DiscoveredServices.Close();
      }
      protected void FireEvent(params object[] args)
      {
         string action = OperationContext.Current.IncomingMessageHeaders.Action;
         string[] slashes = action.Split('/');
         string methodName = slashes[slashes.Length-1];

         FireEvent(methodName,args);
      }
      void FireEvent(string methodName,object[] args)
      {
         T[] subscribers = GetSubscribers();
         Publish(subscribers,methodName,args);
      }

      T[] GetSubscribers()
      {
         IEnumerable<string> announcedAddress  = AnnouncedSubscribers.FindComplement(Scope).Select((address)=>address.Uri.AbsoluteUri);
         IEnumerable<string> discoveredAddress = DiscoveredServices.FindComplement(Scope).Select((address)=>address.Uri.AbsoluteUri);

         IEnumerable<string>  addresses = announcedAddress.Union(discoveredAddress);
         
         List<T> subscribers = new List<T>();
 
         foreach(string address in addresses)
         {
            EndpointAddress endpointAddress = new EndpointAddress(address);
            Binding binding = GetBindingFromAddress(endpointAddress);
            T proxy  = ChannelFactory<T>.CreateChannel(binding,endpointAddress);
            subscribers.Add(proxy);
         }
         return subscribers.ToArray();
      }
      static void Publish(T[] subscribers,string methodName,object[] args)
      {
         WaitCallback fire = (subscriber)=>
                             {
                                using(subscriber as IDisposable)
                                {
                                   Invoke(subscriber as T,methodName,args);
                                }
                             };
         Action<T> queueUp = (subscriber)=>
                             {
                                ThreadPool.QueueUserWorkItem(fire,subscriber);
                             };
         subscribers.ForEach(queueUp);
      }
      static void Invoke(T subscriber,string methodName,object[] args)
      {
         Debug.Assert(subscriber != null);
         Type type = typeof(T);
         MethodInfo methodInfo = type.GetMethod(methodName);
         try
         {
            methodInfo.Invoke(subscriber,args);
         }
         catch(Exception e)
         {
            Trace.WriteLine(e.Message);
         }
      }            
      static Binding GetBindingFromAddress(EndpointAddress address)
      {
         if(address.Uri.Scheme == "net.tcp")
         {
            return Binding;
         }
         if(address.Uri.Scheme == "net.pipe")
         {
            return new NetNamedPipeBinding();
         }
         Debug.Assert(false,"Unsupported binding specified");
         return null;
      }
   }
}