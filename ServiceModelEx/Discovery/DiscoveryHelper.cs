// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;

using Microsoft.ServiceBus;


namespace ServiceModelEx
{
   public static class DiscoveryHelper
   {
      public static void EnableDiscovery(this ServiceHost host,bool enableMEX = true)
      {
         EnableDiscovery(host,null,enableMEX);
      }
      public static void EnableDiscovery(this ServiceHost host,Uri scope,bool enableMEX = true)
      {
         if(host.Description.Endpoints.Count == 0)
         {
            host.AddDefaultEndpoints();
         }
         host.AddServiceEndpoint(new UdpDiscoveryEndpoint());

         ServiceDiscoveryBehavior discovery = new ServiceDiscoveryBehavior();
         discovery.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());
         host.Description.Behaviors.Add(discovery);

         if(enableMEX == true)
         {         
            host.Description.Behaviors.Add(new ServiceMetadataBehavior());

            foreach(Uri baseAddress in host.BaseAddresses)
            {
               Binding binding = null;

               if(baseAddress.Scheme == "net.tcp")
               {
                  binding = MetadataExchangeBindings.CreateMexTcpBinding();
               }
               if(baseAddress.Scheme == "net.pipe")
               {
                  binding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
               }
               if(baseAddress.Scheme == "http")
               {
                  binding = MetadataExchangeBindings.CreateMexHttpBinding();
               }
               if(baseAddress.Scheme == "https")
               {
                  binding = MetadataExchangeBindings.CreateMexHttpsBinding();
               }

               if(baseAddress.Scheme == "sb")
               {
                  binding = new NetTcpRelayBinding();
               }

               Debug.Assert(binding != null);
               if(binding != null)
               {
                  host.AddServiceEndpoint(typeof(IMetadataExchange),binding,"MEX");
               }         
            }
         }
         if(scope != null)
         {
            EndpointDiscoveryBehavior behavior = new EndpointDiscoveryBehavior();
            behavior.Scopes.Add(scope);

            foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
            {
               if(endpoint.IsSystemEndpoint        ||
                  endpoint is DiscoveryEndpoint    || 
                  endpoint is AnnouncementEndpoint ||
                  endpoint is ServiceMetadataEndpoint)
               {
                  continue;
               }
               endpoint.Behaviors.Add(behavior);
            }
         }
      }
      public static Uri AvailableTcpBaseAddress
      {
         get
         {
            string machineName = Environment.MachineName;
            return new Uri("net.tcp://" + machineName + ":" + FindAvailablePort() + "/");
         }
      }

      public static Uri AvailableIpcBaseAddress
      {
         get
         {
            return new Uri("net.pipe://localhost/" + Guid.NewGuid() + "/");
         }
      }
      static int FindAvailablePort()
      {
         Mutex mutex = new Mutex(false,"ServiceModelEx.DiscoveryHelper.FindAvailablePort");
         try
         {
            mutex.WaitOne();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any,0);
            using(Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp))
            {
               socket.Bind(endPoint);
               IPEndPoint local = (IPEndPoint)socket.LocalEndPoint;
               return local.Port;
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      static EndpointAddress[] Discover<T>(int maxResults,Uri scope)
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = new FindCriteria(typeof(T));
         criteria.MaxResults = maxResults;
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         return discovered.Endpoints.Select((endpoint)=>endpoint.Address).ToArray();
      }
      public static EndpointAddress DiscoverAddress<T>(Uri scope = null)
      {
         EndpointAddress[] addresses = Discover<T>(1,scope);
         Debug.Assert(addresses.Length == 1);

         return addresses[0];
      }
      public static EndpointAddress[] DiscoverAddresses<T>(Uri scope = null)
      {
         return Discover<T>(int.MaxValue,scope);
      }

      public static Binding DiscoverBinding<T>(Uri scope = null)
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

         return endpoints[0].Binding;
      }
      public static Uri[] LookupScopes(ServiceEndpoint endpoint)
      {
         Uri[] scopes = new Uri[]{};
         EndpointDiscoveryBehavior behavior = endpoint.Behaviors.Find<EndpointDiscoveryBehavior>();
         if(behavior != null)
         {
            if(behavior.Scopes.Count > 0)
            {
               scopes = behavior.Scopes.ToArray();
            }
         }
         return scopes;
      }
      internal static AnnouncementEventArgs CreateAnnouncementArgs(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         Type type = typeof(AnnouncementEventArgs);
         ConstructorInfo constructor = type.GetConstructors(BindingFlags.Instance|BindingFlags.NonPublic)[0];

         ContractDescription contract = new ContractDescription(contractName,contractNamespace);

         ServiceEndpoint endpoint = new ServiceEndpoint(contract,null,new EndpointAddress(address));
         EndpointDiscoveryMetadata metadata = EndpointDiscoveryMetadata.FromServiceEndpoint(endpoint);

         return constructor.Invoke(new object[]{null,metadata}) as AnnouncementEventArgs;
      }
      internal static FindResponse CreateFindResponse()
      {
         Type type = typeof(FindResponse);
         ConstructorInfo constructor = type.GetConstructors(BindingFlags.Instance|BindingFlags.NonPublic)[0];

         return constructor.Invoke(null) as FindResponse;
      }    
   }
}


