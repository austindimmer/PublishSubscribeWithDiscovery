// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using Microsoft.ServiceBus;


namespace ServiceModelEx.ServiceBus
{
   public partial class DiscoverableServiceHost : ServiceHost,IServiceBusProperties
   {
      public const string AnnouncementsPath = "AvailabilityAnnouncements";
      public const string DiscoveryPath     = "DiscoveryRequests";
      
      Uri m_AnnouncementsAddress;
      Uri m_DiscoveryAddress;
      ServiceHost m_DiscoveryHost;

      NetOnewayRelayBinding m_AnnouncementsBinding;
      NetEventRelayBinding  m_DiscoveryRequestBinding;
      NetOnewayRelayBinding m_DiscoveryResponseBinding;

      protected string Namespace
      {
         get
         {
            if(Description.Endpoints.Count > 0)
            {
               foreach(ServiceEndpoint endpoint in Description.Endpoints)
               {
                  if(endpoint.Address.Uri.AbsoluteUri.Contains("servicebus.windows.net"))
                  {
                     return ServiceBusHelper.ExtractNamespace(endpoint.Address.Uri);
                  }
               }
            }
            if(BaseAddresses.Count > 0)
            {
               return ServiceBusHelper.ExtractNamespace(BaseAddresses[0]);
            }
            return null;
         }
      }


      bool IsAnnouncing
      {
         get
         {
            ServiceDiscoveryBehavior behavior = Description.Behaviors.Find<ServiceDiscoveryBehavior>();
            if(behavior != null)
            {
               return behavior.AnnouncementEndpoints.Any();
            }
            return false;
         }
      }

      bool IsDiscoverable
      {
         get
         {
            if(Description.Behaviors.Find<ServiceDiscoveryBehavior>() != null)
            {
               return Description.Endpoints.Any(endpoint => endpoint is DiscoveryEndpoint);
            }
            return false;
         }
      }
      
      public Uri AnnouncementsAddress
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_AnnouncementsAddress == null)
            {
               m_AnnouncementsAddress = ServiceBusEnvironment.CreateServiceUri("sb",Namespace,AnnouncementsPath);
            }
            return m_AnnouncementsAddress;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            Debug.Assert(value.Scheme == "sb");
            string serviceNamespace = ServiceBusHelper.ExtractNamespace(value);

            if(Namespace != null)
            {
               Debug.Assert(serviceNamespace == Namespace);
            }

            m_AnnouncementsAddress = value;
         }
      }       
      public Uri DiscoveryAddress
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_DiscoveryAddress == null)
            {
               m_DiscoveryAddress = ServiceBusEnvironment.CreateServiceUri("sb",Namespace,DiscoveryPath);
            }
            return m_DiscoveryAddress;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            Debug.Assert(value.Scheme == "sb");
            string serviceNamespace = ServiceBusHelper.ExtractNamespace(value);

            Debug.Assert(serviceNamespace == Namespace);

            m_DiscoveryAddress = value;
         }
      }


      public NetOnewayRelayBinding AnnouncementsBinding
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_AnnouncementsBinding == null)
            {
               m_AnnouncementsBinding = new NetOnewayRelayBinding();
            }
            return m_AnnouncementsBinding;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            m_AnnouncementsBinding = value;
         }
      }
      public NetEventRelayBinding DiscoveryRequestBinding
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_DiscoveryRequestBinding == null)
            {
               m_DiscoveryRequestBinding = new NetEventRelayBinding();
            }
            return m_DiscoveryRequestBinding;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            m_DiscoveryRequestBinding = value;
         }
      }

      public NetOnewayRelayBinding DiscoveryResponseBinding
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_DiscoveryResponseBinding == null)
            {
               m_DiscoveryResponseBinding = new NetOnewayRelayBinding();
            }
            return m_DiscoveryResponseBinding;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            m_DiscoveryResponseBinding = value;
         }
      }
      
      public DiscoverableServiceHost(object singletonInstance,params Uri[] baseAddresses) : base(singletonInstance,baseAddresses)
      {}

      public DiscoverableServiceHost(Type serviceType,params Uri[] baseAddresses) : base(serviceType,baseAddresses)
      {}

      public void EnableServiceBusDiscovery(bool enableMEX = true)
      {
         EnableServiceBusDiscovery(null,enableMEX);
      }
     
      void EnableServiceBusDiscovery(Uri scope,bool enableMEX,Uri[] baseAddresses)
      {
         Debug.Assert(baseAddresses.Any(address=>address.Scheme == "sb"));

         if(Description.Endpoints.Count == 0)
         {
            this.AddServiceBusDefaultEndpoints(baseAddresses);
         }

         AddServiceEndpoint(new UdpDiscoveryEndpoint());

         ServiceDiscoveryBehavior discovery = new ServiceDiscoveryBehavior();
         discovery.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());
         Description.Behaviors.Add(discovery);

         if(enableMEX == true)
         {         
            Description.Behaviors.Add(new ServiceMetadataBehavior());

            foreach(Uri baseAddress in baseAddresses)
            {
               if(baseAddress.Scheme == "sb")
               {
                  string address = baseAddress.AbsoluteUri;

                  if(address.EndsWith("/") == false)
                  {
                     address += "/";
                  }  
                  address += "MEX";
                  AddServiceEndpoint(typeof(IMetadataExchange),new NetTcpRelayBinding(),address);
                  break;
               }         
            }
            if(scope != null)
            {
               EndpointDiscoveryBehavior behavior = new EndpointDiscoveryBehavior();
               behavior.Scopes.Add(scope);

               foreach(ServiceEndpoint endpoint in Description.Endpoints)
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
      }
      public void EnableServiceBusDiscovery(Uri scope,bool enableMEX = true)
      {
         EnableServiceBusDiscovery(scope,enableMEX,BaseAddresses.ToArray());
      }
 
      public static DiscoverableServiceHost CreateDiscoverableHost(Type serviceType,Uri baseAddress,Uri scope = null) 
      {
         DiscoverableServiceHost host = new DiscoverableServiceHost(serviceType);
         host.EnableServiceBusDiscovery(scope,true,new Uri[]{baseAddress});
         return host;
      }

      void EnableDiscovery()
      {
         Debug.Assert(State != CommunicationState.Opened);

         IEndpointBehavior registryBehavior = new ServiceRegistrySettings(DiscoveryType.Public);
         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {
            endpoint.Behaviors.Add(registryBehavior);
         }


         //Launch the service to monitor discovery requests

         DiscoveryRequestService discoveryService = new DiscoveryRequestService(Description.Endpoints.ToArray());
         discoveryService.DiscoveryResponseBinding = DiscoveryResponseBinding;

         m_DiscoveryHost = new ServiceHost(discoveryService);

         m_DiscoveryHost.AddServiceEndpoint(typeof(IServiceBusDiscovery),DiscoveryRequestBinding,DiscoveryAddress.AbsoluteUri);

         TransportClientEndpointBehavior credentials = (this as IServiceBusProperties).Credential;
         m_DiscoveryHost.Description.Endpoints[0].Behaviors.Add(credentials);

         m_DiscoveryHost.Description.Endpoints[0].Behaviors.Add(registryBehavior);

         m_DiscoveryHost.Open();
      }

      IServiceBusAnnouncements CreateAvailabilityAnnouncementsClient()
      {
         TransportClientEndpointBehavior credentials = (this as IServiceBusProperties).Credential;
       
         ChannelFactory<IServiceBusAnnouncements> factory = new ChannelFactory<IServiceBusAnnouncements>(AnnouncementsBinding,new EndpointAddress(AnnouncementsAddress));
         
         factory.Endpoint.Behaviors.Add(credentials);

         return factory.CreateChannel();
      }
      protected override void OnOpening()
      {
         if(IsDiscoverable)
         {
            EnableDiscovery();
         }

         base.OnOpening();
      }
      protected override void OnOpened()
      {
         base.OnOpened();

         if(IsAnnouncing)
         {
            IServiceBusAnnouncements proxy = CreateAvailabilityAnnouncementsClient();
            PublishAvailabilityEvent(proxy.OnHello);
         }
      }
      protected override void OnClosed()
      {
         if(IsAnnouncing)
         {
            IServiceBusAnnouncements proxy = CreateAvailabilityAnnouncementsClient();
            PublishAvailabilityEvent(proxy.OnBye);
         }

         if(m_DiscoveryHost != null)
         {
            m_DiscoveryHost.Close();
         }

         base.OnClosed();
      }
      protected override void OnAbort()
      {
         if(m_DiscoveryHost != null)
         {
            m_DiscoveryHost.Abort();
         }
         base.OnAbort();
      }
      void PublishAvailabilityEvent(Action<Uri,string,string,Uri[]> notification)
      {
         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {
            if(endpoint is DiscoveryEndpoint || endpoint is ServiceMetadataEndpoint)
            {
               continue;
            }
            Uri[] scopes = DiscoveryHelper.LookupScopes(endpoint);

            WaitCallback fire = delegate
                                {
                                   try
                                   {
                                      notification(endpoint.Address.Uri,endpoint.Contract.Name,endpoint.Contract.Namespace,scopes);
                                      (notification.Target as ICommunicationObject).Close();
                                   }
                                   catch
                                   {}
                                };
            ThreadPool.QueueUserWorkItem(fire);        
         }
      }
      TransportClientEndpointBehavior IServiceBusProperties.Credential
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            TransportClientEndpointBehavior credentials = null;

            foreach(ServiceEndpoint endpoint in Description.Endpoints)
            {
               credentials = endpoint.Behaviors.Find<TransportClientEndpointBehavior>();
               if(credentials != null)
               {
                  break;
               }
            }
            Debug.Assert(credentials != null);

            return credentials;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            Debug.Assert(State != CommunicationState.Opened);
            foreach(ServiceEndpoint endpoint in Description.Endpoints)
            {
               Debug.Assert(endpoint.Behaviors.Contains(typeof(TransportClientEndpointBehavior)) == false,"Do not add credentials mutiple times");
               endpoint.Behaviors.Add(value);
            }
         }
      }
      Uri[] IServiceBusProperties.Addresses
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            return Addresses;
         }
      }
      protected virtual Uri[] Addresses
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            List<Uri> addresses = new List<Uri>();

            foreach(ServiceEndpoint endpoint in Description.Endpoints)
            {
               addresses.Add(endpoint.Address.Uri);
            }
            return addresses.ToArray();
         }
      }
   }
}

