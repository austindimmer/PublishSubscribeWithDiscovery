// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading.Tasks;


namespace ServiceModelEx.TcpDiscovery
{
   public static partial class DiscoveryFactory
   {
      internal static Binding Binding
      {
         get
         {
            NetTcpBinding binding = ServiceModelEx.DiscoveryFactory.InferBindingFromUri(new Uri("net.tcp://temp")) as NetTcpBinding;
            binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.FromSeconds(5);//We fast fast failure - discovery endpoints should take next to nothing
            return binding;
         }
      }
      public static ServiceHost<T> CreateDiscoverableHost<T>(Uri scope = null) where T : class
      {
         ServiceHost<T> host = ServiceModelEx.DiscoveryFactory.CreateDiscoverableHost<T>(scope);

         DiscoveryRequestService requestService = new DiscoveryRequestService(host.Description.Endpoints.UnsafeToArray<ServiceEndpoint>());

         ServiceHost requestHost = new ServiceHost(requestService);
         Uri requestServiceAddress = new Uri(DiscoveryHelper.AvailableTcpBaseAddress.AbsoluteUri + typeof(T) + "/" + "DiscoveryEndpoint");

         requestHost.AddServiceEndpoint(typeof(IDiscovery),DiscoveryFactory.Binding,requestServiceAddress);
         
         host.Opened += delegate
                        {
                           try
                           {
                              requestHost.Open();

                              IDiscoverySubscription pubsubProxy = ChannelFactory<IDiscoverySubscription>.CreateChannel(DiscoveryFactory.Binding,Address.DiscoverySubscription);
                              pubsubProxy.Subscribe(requestServiceAddress.AbsoluteUri);
                              (pubsubProxy as ICommunicationObject).Close(); 

                           }
                           catch
                           {
                              Trace.WriteLine("Could not subscribe to discovery requests for service " + typeof(T));
                           }

                           IAnnouncements announcementsProxy = ChannelFactory<IAnnouncements>.CreateChannel(DiscoveryFactory.Binding,Address.Announcements);
                           PublishAvailabilityEvent(host.Description.Endpoints.ToArray(),announcementsProxy.OnHello);
                        };

         host.Closing += delegate
                         {
                            try
                            {
                               IDiscoverySubscription pubsubProxy = ChannelFactory<IDiscoverySubscription>.CreateChannel(DiscoveryFactory.Binding,Address.DiscoverySubscription);
                               pubsubProxy.Unsubscribe(requestServiceAddress.AbsoluteUri);
                               (pubsubProxy as ICommunicationObject).Close(); 
                               requestHost.Close();
                            }
                            catch
                            {
                               Trace.WriteLine("Could not unsubscribe to discovery requests for service " + typeof(T));
                            }
                            IAnnouncements announcementsProxy = ChannelFactory<IAnnouncements>.CreateChannel(DiscoveryFactory.Binding,Address.Announcements);
                            PublishAvailabilityEvent(host.Description.Endpoints.ToArray(),announcementsProxy.OnBye);
                         };
         return host;
      }

      public static ServiceHost CreateDiscoveryService(uint port = 0)
      {
         if(port > 0)
         {
            Address.Port = port;
         }

         Uri baseAddress = new Uri("net.tcp://" + Address.DiscoveryServer + ":" + Address.Port +"/");
         
         ServiceHost host = new ServiceHost(typeof(DiscoveryService),baseAddress);
         
         //At this point, any endpoint in config is already loaded up.

         if(host.Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IAnnouncementsSubscription)) == false)
         {
            host.AddServiceEndpoint(typeof(IAnnouncementsSubscription),Binding,Address.AnnouncementsSubscription.Uri.AbsoluteUri);
         }

         if(host.Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IAnnouncements)) == false)
         {
            host.AddServiceEndpoint(typeof(IAnnouncements),Binding,Address.Announcements.Uri.AbsoluteUri);
         }
        
         if(host.Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IDiscovery)) == false)
         {
            host.AddServiceEndpoint(typeof(IDiscovery),Binding,Address.DiscoveryService.Uri.AbsoluteUri);
         }
         
         if(host.Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IDiscoverySubscription)) == false)
         {
            host.AddServiceEndpoint(typeof(IDiscoverySubscription),Binding,Address.DiscoverySubscription.Uri.AbsoluteUri);
         }

         return host;
      }

      static void PublishAvailabilityEvent(ServiceEndpoint[] endpoints,Action<Uri,string,string,Uri[]> notification)
      {
         Action<ServiceEndpoint> notify = (endpoint)=>
                                          {
                                             if(endpoint is DiscoveryEndpoint || endpoint is ServiceMetadataEndpoint || endpoint.Contract.ContractType == typeof(IMetadataExchange))
                                             {
                                                return;
                                             }
                                             Uri[] scopes = DiscoveryHelper.LookupScopes(endpoint);

                                             notification(endpoint.Address.Uri,endpoint.Contract.Name,endpoint.Contract.Namespace,scopes);                                                
                                          };

         Action publish = ()=>
                          {
                             try
                             {
                                endpoints.ParallelForEach(notify);
                             }
                             catch
                             {           
                                Trace.WriteLine("Could not announce availability of service");
                             } 
                             finally
                             {           
                                (notification.Target as ICommunicationObject).Close();
                             }  
                          };
         Task.Run(publish);
      }
     
      public static class Address
      {
         public static string DiscoveryServer
         { 
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
         }

         internal static uint Port
         {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
         }

         static Address()
         {
            Port = 808;
            DiscoveryServer = "localhost";
         }
         
         [MethodImpl(MethodImplOptions.Synchronized)]
         static EndpointAddress GetAddressFromClientConfig(Type type,string endpointName = null)
         {
            Debug.Assert(type.IsInterface);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ServiceModelSectionGroup sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);

            EndpointAddress address = null;
            foreach(ChannelEndpointElement endpointElement in sectionGroup.Client.Endpoints)
            {
               if((String.IsNullOrEmpty(endpointName)) && (endpointElement.Contract == type.ToString()))
               {
                  address = new EndpointAddress(endpointElement.Address);
                  break;
               }
               else
               {
                  if(endpointElement.Name == endpointName)
                  {
                     address = new EndpointAddress(endpointElement.Address);
                     break;
                  }
               }
            }
            return address;
         }
 
         //Discovery service address management      
         static EndpointAddress m_DiscoveryServiceAddress;

         public static EndpointAddress DiscoveryService
         {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
               if(m_DiscoveryServiceAddress == null)
               {
                  m_DiscoveryServiceAddress = GetDiscoveryServiceFromConfig();
                  if(m_DiscoveryServiceAddress == null)
                  {
                     m_DiscoveryServiceAddress = new EndpointAddress("net.tcp://"+DiscoveryServer+":"+Port+"/DiscoveryService/Discovery");
                  }
               }
               return m_DiscoveryServiceAddress;
            }
         }
         
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetDiscoveryService(EndpointAddress address)
         {
            m_DiscoveryServiceAddress = address;
         }
         
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetDiscoveryService(string endpointName = null)
         {
            m_DiscoveryServiceAddress = GetDiscoveryServiceFromConfig(endpointName);
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         internal static EndpointAddress GetDiscoveryServiceFromConfig(string endpointName = null)
         {
            return GetAddressFromClientConfig(typeof(IDiscovery),endpointName);
         }

         //Discovery subscription address management      
         static EndpointAddress m_DiscoverySubscriptionAddress;

         public static EndpointAddress DiscoverySubscription
         {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
               if(m_DiscoverySubscriptionAddress == null)
               {
                  m_DiscoverySubscriptionAddress = GetSubscriptionFromConfig();
                  if(m_DiscoverySubscriptionAddress == null)
                  {
                     m_DiscoverySubscriptionAddress = new EndpointAddress("net.tcp://"+DiscoveryServer+":"+Port+"/DiscoveryService/DiscoverySubscription");
                  }
               }
               return m_DiscoverySubscriptionAddress;
            }
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetDiscoverySubscriptionService(EndpointAddress address)
         {
            m_DiscoverySubscriptionAddress = address;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetDiscoverySubscriptionService(string endpointName = null)
         {
            m_DiscoverySubscriptionAddress = GetSubscriptionFromConfig(endpointName);
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         internal static EndpointAddress GetSubscriptionFromConfig(string endpointName = null)
         {
            return GetAddressFromClientConfig(typeof(IDiscoverySubscription),endpointName);
         }
         
         //Announcements subscription address management      
         static EndpointAddress m_AnnouncementsSubscriptionAddress;
         public static EndpointAddress AnnouncementsSubscription
         {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
               if(m_AnnouncementsSubscriptionAddress == null)
               {
                  m_AnnouncementsSubscriptionAddress = GetAnnouncementsSubscriptionFromConfig();
                  if(m_AnnouncementsSubscriptionAddress == null)
                  {
                     m_AnnouncementsSubscriptionAddress = new EndpointAddress("net.tcp://"+DiscoveryServer+":"+Port+"/DiscoveryService/AnnouncementsSubscription");
                  }
               }
               return m_AnnouncementsSubscriptionAddress;
            }
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetAnnouncementsSubscription(EndpointAddress address)
         {
            m_AnnouncementsSubscriptionAddress = address;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetAnnouncementsSubscription(string endpointName = null)
         {
            m_AnnouncementsSubscriptionAddress = GetAnnouncementsSubscriptionFromConfig(endpointName);
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         internal static EndpointAddress GetAnnouncementsSubscriptionFromConfig(string endpointName = null)
         {
            return GetAddressFromClientConfig(typeof(IAnnouncementsSubscription),endpointName);
         }

         //Announcements address management      
         static EndpointAddress m_AnnouncementsAddress;
         public static EndpointAddress Announcements
         {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
               if(m_AnnouncementsAddress == null)
               {
                  m_AnnouncementsAddress = GetAnnouncementsFromConfig();
                  if(m_AnnouncementsAddress == null)
                  {
                     m_AnnouncementsAddress = new EndpointAddress("net.tcp://"+DiscoveryServer+":"+Port+"/DiscoveryService/Announcements");
                  }
               }
               return m_AnnouncementsAddress;
            }
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetAnnouncements(EndpointAddress address)
         {
            m_AnnouncementsAddress = address;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         public static void SetAnnouncements(string endpointName = null)
         {
            m_AnnouncementsAddress = GetAnnouncementsFromConfig(endpointName);
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         internal static EndpointAddress GetAnnouncementsFromConfig(string endpointName = null)
         {
            return GetAddressFromClientConfig(typeof(IAnnouncements),endpointName);
         }
      }

      public static T CreateChannel<T>(Uri scope = null) where T : class
      {
         DuplexDiscoveryClient discoveryClient = new DuplexDiscoveryClient(Address.DiscoveryService);

         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();
         criteria.MaxResults = 1;
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);

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
         DuplexDiscoveryClient discoveryClient = new DuplexDiscoveryClient(Address.DiscoveryService);
         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();

         FindResponse discovered = discoveryClient.Find(criteria);

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
         DuplexDiscoveryClient discoveryClient = new DuplexDiscoveryClient(Address.DiscoveryService);
         FindCriteria criteria = new FindCriteria(typeof(T));
         FindResponse discovered = discoveryClient.Find(criteria);

         if(discovered.Endpoints.Count == 0)
         {
            return new T[]{};
         }
         List<T> list = new List<T>();

         foreach(EndpointDiscoveryMetadata endpoint in discovered.Endpoints)
         {
            Binding binding = ServiceModelEx.DiscoveryFactory.InferBindingFromUri(endpoint.Address.Uri);           
            T proxy = ChannelFactory<T>.CreateChannel(binding,endpoint.Address);
            list.Add(proxy);
         }
         return list.ToArray();
      }
      
      [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,ConcurrencyMode = ConcurrencyMode.Multiple,UseSynchronizationContext = false)]
      class DiscoveryRequestService : IDiscovery
      {
         readonly ServiceEndpoint[] Endpoints;

         public DiscoveryRequestService(ServiceEndpoint[] endpoints)
         {
            Endpoints = endpoints;
         }
         public void OnDiscoveryRequest(string contractName,string contractNamespace,Uri[] scopesToMatch)
         {
            IDiscoveryCallback callback = OperationContext.Current.GetCallbackChannel<IDiscoveryCallback>();

            foreach(ServiceEndpoint endpoint in Endpoints)
            {
               if(endpoint.Contract.Name == contractName && endpoint.Contract.Namespace == contractNamespace)
               {
                  Uri[] scopes = DiscoveryHelper.LookupScopes(endpoint);

                  if(scopesToMatch != null)
                  {
                     bool scopesMatched = true;
                     foreach(Uri scope in scopesToMatch)
                     {
                        if(scopes.Any(uri => uri.AbsoluteUri == scope.AbsoluteUri) == false)
                        {
                           scopesMatched = false;
                           break;
                        }
                     }
                     if(scopesMatched == false)
                     {
                        continue;
                     }
                  }

                  callback.OnDiscoveryResponse(endpoint.Address.Uri,contractName,contractNamespace,scopes);
               }
            }
         }
      }
   }
}


