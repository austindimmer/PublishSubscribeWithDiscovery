// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Xml;
using System.Runtime.CompilerServices;

namespace ServiceModelEx
{
   public class AnnouncementSink<T> : AddressesContainer<T> where T : class
   {
      readonly ServiceHost m_Host;

      public event Action<string,Uri[]> OnlineAnnouncementReceived  = delegate{};
      public event Action<string,Uri[]> OfflineAnnouncementReceived = delegate{};

      public AnnouncementSink() 
      {
         AnnouncementService announcementService = new AnnouncementService();
         m_Host = new ServiceHost(announcementService);
         m_Host.Description.Behaviors.Find<ServiceBehaviorAttribute>().UseSynchronizationContext = false;

         m_Host.AddServiceEndpoint(new UdpAnnouncementEndpoint());

         announcementService.OnlineAnnouncementReceived  += OnHello;  
         announcementService.OfflineAnnouncementReceived += OnBye;
       }
      public override void Open()
      {
         m_Host.Open();
      }      
      public override void Close()
      {
         m_Host.Close();
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      protected void OnHello(object sender,AnnouncementEventArgs args)
      {
         if(Dictionary.ContainsKey(args.EndpointDiscoveryMetadata.Address))
         {
            return;
         }
         foreach(XmlQualifiedName contract in args.EndpointDiscoveryMetadata.ContractTypeNames)
         {
            if(contract.Name == typeof(T).Name && contract.Namespace == Namespace)
            {
               Dictionary[args.EndpointDiscoveryMetadata.Address] = args.EndpointDiscoveryMetadata.Scopes.ToArray();
               PublishAvailabilityEvent(OnlineAnnouncementReceived,args.EndpointDiscoveryMetadata.Address.Uri.AbsoluteUri,args.EndpointDiscoveryMetadata.Scopes.ToArray());
            }
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      protected void OnBye(object sender,AnnouncementEventArgs args)
      {
         if(Dictionary.ContainsKey(args.EndpointDiscoveryMetadata.Address))
         {
            Dictionary.Remove(args.EndpointDiscoveryMetadata.Address);
         }
         foreach(XmlQualifiedName contract in args.EndpointDiscoveryMetadata.ContractTypeNames)
         {
            if(contract.Name == typeof(T).Name && contract.Namespace == Namespace)
            {
               PublishAvailabilityEvent(OfflineAnnouncementReceived,args.EndpointDiscoveryMetadata.Address.Uri.AbsoluteUri,args.EndpointDiscoveryMetadata.Scopes.ToArray());

               if(Dictionary.ContainsKey(args.EndpointDiscoveryMetadata.Address) == false)
               {
                  Trace.WriteLine("Received 'bye' announcement that did not have matching 'hello' first from address " + args.EndpointDiscoveryMetadata.Address.Uri.AbsoluteUri);
               }
            }
         }
      }
      protected void PublishAvailabilityEvent(Action<string,Uri[]> notification,string address,Uri[] scopes)
      {         
         Delegate[] subscribers = notification.GetInvocationList();
         Action<Delegate> publish = (subscriber=>subscriber.DynamicInvoke(address,scopes));
         subscribers.ForEachAsync(publish);
      }
   }
}