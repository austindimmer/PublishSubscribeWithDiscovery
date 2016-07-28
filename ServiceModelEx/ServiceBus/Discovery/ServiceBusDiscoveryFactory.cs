// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;


namespace ServiceModelEx.ServiceBus
{
   public static class ServiceBusDiscoveryFactory
   {
      public static T CreateChannel<T>(string serviceNamespace,string secret,Uri scope = null) where T : class
      {
         ServiceBusDiscoveryClient discoveryClient = new ServiceBusDiscoveryClient(serviceNamespace,secret);

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
 
         ServiceEndpoint[] endpoints = ServiceBusMetadataHelper.GetEndpoints(mexAddress.AbsoluteUri,typeof(T),secret);
         Debug.Assert(endpoints.Length == 1);

         Binding binding = endpoints[0].Binding;
         EndpointAddress address = endpoints[0].Address;

         ChannelFactory<T> factory = new ChannelFactory<T>(binding,address);
         factory.SetServiceBusCredentials(secret);

         return factory.CreateChannel();
      }         
      public static T[] CreateChannels<T>(string serviceNamespace,string secret,Uri scope = null) where T : class
      {
         ServiceBusDiscoveryClient discoveryClient = new ServiceBusDiscoveryClient(serviceNamespace,secret);
         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         if(discovered.Endpoints.Count == 0)
         {
            return new T[]{};
         }
         Debug.Assert(discovered.Endpoints.Count > 0);

         List<T> list = new List<T>();

         foreach(EndpointDiscoveryMetadata mexEndpoint in discovered.Endpoints)
         {
            ServiceEndpoint[] endpoints = ServiceBusMetadataHelper.GetEndpoints(mexEndpoint.Address.Uri.AbsoluteUri,typeof(T),secret);
            foreach(ServiceEndpoint endpoint in endpoints)
            {
               ChannelFactory<T> factory = new ChannelFactory<T>(endpoint.Binding,endpoint.Address);
               factory.SetServiceBusCredentials(secret);

               T proxy = factory.CreateChannel();
               list.Add(proxy);
            }
         }
         Debug.Assert(list.Count > 0);
         return list.ToArray();
      }
   }
}


