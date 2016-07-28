// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;

namespace ServiceModelEx.ServiceBus
{
   public static class ServiceBusDiscoveryHelper
   {
      public static EndpointAddress DiscoverAddress<T>(string serviceNamespace,string secret,Uri scope = null)
      {
         ServiceBusDiscoveryClient discoveryClient = new ServiceBusDiscoveryClient(serviceNamespace,secret);
         FindCriteria criteria = new FindCriteria(typeof(T));
         criteria.MaxResults = 1;
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }

         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         Debug.Assert(discovered.Endpoints.Count == 1);

         return discovered.Endpoints[0].Address;
      }
      public static EndpointAddress[] DiscoverAddresses<T>(string serviceNamespace,string secret,Uri scope = null)
      {
         ServiceBusDiscoveryClient discoveryClient = new ServiceBusDiscoveryClient(serviceNamespace,secret);
         FindCriteria criteria = new FindCriteria(typeof(T));
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         return discovered.Endpoints.Select((endpoint)=>endpoint.Address).ToArray();
      }

      public static Binding DiscoverBinding<T>(string serviceNamespace,string secret,Uri scope = null)
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

         return endpoints[0].Binding;
      }
   }
}


