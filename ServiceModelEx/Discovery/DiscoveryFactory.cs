// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;


namespace ServiceModelEx
{
   public static class DiscoveryFactory
   {
      public static ServiceHost<T> CreateDiscoverableHost<T>(bool supportIpc = false) where T : class
      {
         return CreateDiscoverableHost<T>(null,supportIpc);
      }
      public static ServiceHost<T> CreateDiscoverableHost<T>(Uri scope,bool supportIpc = false) where T : class
      {
         ServiceHost<T> host;
         if(supportIpc == true)
         {
            host = new ServiceHost<T>(DiscoveryHelper.AvailableIpcBaseAddress,new Uri(DiscoveryHelper.AvailableTcpBaseAddress.AbsoluteUri+"/"));
         }
         else
         {
             host = new ServiceHost<T>(new Uri(DiscoveryHelper.AvailableTcpBaseAddress.AbsoluteUri+"/"));
         }
         host.EnableDiscovery(scope);

         return host;
      }

      public static T CreateChannel<T>(Uri scope = null) where T : class
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());

         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();
         criteria.MaxResults = 1;
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         Debug.Assert(discovered.Endpoints.Count == 1);
         Uri mexAddress = discovered.Endpoints[0].Address.Uri;
 
         ServiceEndpoint[] endpoints = MetadataHelper.GetEndpoints(mexAddress.AbsoluteUri,typeof(T));
         Debug.Assert(endpoints.Length == 1);

         Binding binding = endpoints[0].Binding;
         EndpointAddress address = endpoints[0].Address;

         return ChannelFactory<T>.CreateChannel(binding,address);
      }         
      public static T[] CreateChannels<T>(bool inferBinding = true) where T : class
      {
         if(inferBinding)
         {
            return CreateInferedChannels<T>();
         }
         else
         {
            return CreateChannelsFromMex<T>();
         }
      }

      static T[] CreateChannelsFromMex<T>(Uri scope = null) where T : class  
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();

         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         if(discovered.Endpoints.Count == 0)
         {
            return new T[]{};
         }

         List<T> list = new List<T>();

         foreach(EndpointDiscoveryMetadata mexEndpoint in discovered.Endpoints)
         {
            ServiceEndpoint[] endpoints = MetadataHelper.GetEndpoints(mexEndpoint.Address.Uri.AbsoluteUri,typeof(T));
            foreach(ServiceEndpoint endpoint in endpoints)
            {
               T proxy = ChannelFactory<T>.CreateChannel(endpoint.Binding,endpoint.Address);
               list.Add(proxy);
            }
         }
         Debug.Assert(list.Count > 0);
         return list.ToArray();
      }

      static T[] CreateInferedChannels<T>() where T : class
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = new FindCriteria(typeof(T));
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         if(discovered.Endpoints.Count == 0)
         {
            return new T[]{};
         }
         List<T> list = new List<T>();

         foreach(EndpointDiscoveryMetadata endpoint in discovered.Endpoints)
         {
            Binding binding = InferBindingFromUri(endpoint.Address.Uri);           
            T proxy = ChannelFactory<T>.CreateChannel(binding,endpoint.Address);
            list.Add(proxy);
         }
         return list.ToArray();
      }
      
      static internal Binding InferBindingFromUri(Uri address)
      {
         switch(address.Scheme)
         {
            case "net.tcp":
            {
               NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.Transport,true);
               tcpBinding.TransactionFlow = true;
               return tcpBinding;
            }
            case "net.pipe":
            {
               NetNamedPipeBinding ipcBinding = new NetNamedPipeBinding();
               ipcBinding.TransactionFlow = true;
               return ipcBinding;
            }
            case "net.msmq":
            {
               NetMsmqBinding msmqBinding = new NetMsmqBinding();
               msmqBinding.Security.Transport.MsmqProtectionLevel = ProtectionLevel.EncryptAndSign;
               return msmqBinding;
            }
            default:
            {
               throw new InvalidOperationException("Can only create a channel over TCP/IPC/MSMQ bindings");
            }
         }   
      }
   }
}


