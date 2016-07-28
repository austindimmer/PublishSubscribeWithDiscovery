// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using Microsoft.ServiceBus;
using System.Runtime.CompilerServices;

namespace ServiceModelEx.ServiceBus
{
   [ServiceBehavior(UseSynchronizationContext = false,InstanceContextMode = InstanceContextMode.Single)]
   public class ServiceBusAnnouncementSink<T> : AnnouncementSink<T>,IServiceBusAnnouncements,IServiceBusProperties where T : class
   {
      Uri m_AnnouncementsAddress;
      NetEventRelayBinding m_AnnouncementsBinding;

      readonly ServiceHost Host;
      readonly string ServiceNamespace;
      readonly TokenProvider TokenProvider;

      public ServiceBusAnnouncementSink(string serviceNamespace,string secret) : this(serviceNamespace,ServiceBusHelper.DefaultIssuer,secret)
      {}

      public ServiceBusAnnouncementSink(string serviceNamespace,string issuer,string secret) : this(serviceNamespace,TokenProvider.CreateSharedSecretTokenProvider(issuer,secret))
      {}
      public ServiceBusAnnouncementSink(string serviceNamespace,TokenProvider tokenProvider)
      {
         Host = new ServiceHost(this);
         Host.SetServiceBusCredentials(tokenProvider);
         ServiceNamespace = serviceNamespace;
         TokenProvider = tokenProvider;
      }

      public NetEventRelayBinding AnnouncementsBinding
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_AnnouncementsBinding == null)
            {
               m_AnnouncementsBinding = new NetEventRelayBinding();
            }
            return m_AnnouncementsBinding;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            m_AnnouncementsBinding = value;
         }
      }

      public Uri AnnouncementsAddress
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_AnnouncementsAddress == null)
            {
               m_AnnouncementsAddress = ServiceBusEnvironment.CreateServiceUri("sb",ServiceNamespace,DiscoverableServiceHost.AnnouncementsPath);
            }
            return m_AnnouncementsAddress;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            Debug.Assert(value.Scheme == "sb");
            m_AnnouncementsAddress = value;
         }
      }
      public override void Open()
      {
         base.Open();         
         
         Host.AddServiceEndpoint(typeof(IServiceBusAnnouncements),AnnouncementsBinding,AnnouncementsAddress.AbsoluteUri);
         Host.Description.Endpoints[0].Behaviors.Add(new ServiceRegistrySettings(DiscoveryType.Public));
         Host.SetServiceBusCredentials(TokenProvider);
         Host.Open();
      }
      public override void Close()
      {
         try
         {
            Host.Close();
         }
         catch
         {
            throw;
         }
         finally
         {
            base.Close();
         }
      }

      void IServiceBusAnnouncements.OnHello(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         AnnouncementEventArgs args = DiscoveryHelper.CreateAnnouncementArgs(address,contractName,contractNamespace,scopes);
         OnHello(this,args);
      }

      void IServiceBusAnnouncements.OnBye(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         AnnouncementEventArgs args = DiscoveryHelper.CreateAnnouncementArgs(address,contractName,contractNamespace,scopes);        
         OnBye(this,args);
      }
   
      TransportClientEndpointBehavior IServiceBusProperties.Credential
      {
      	get 
      	{ 
      		return Host.Description.Endpoints[0].Behaviors.Find<TransportClientEndpointBehavior>(); 
      	}
      	set 
      	{ 
      		Debug.Assert(Host.State != CommunicationState.Opened);
            IEndpointBehavior behavior = Host.Description.Endpoints[0].Behaviors.Find<TransportClientEndpointBehavior>();
            if(behavior != null)
            {
               Host.Description.Endpoints[0].Behaviors.Remove(behavior);
            }
            Host.Description.Endpoints[0].Behaviors.Add(value);
      	}
      }

      Uri[] IServiceBusProperties.Addresses
      {
      	get 
         { 
            return new Uri[]{AnnouncementsAddress};
         }
      }
   }
}