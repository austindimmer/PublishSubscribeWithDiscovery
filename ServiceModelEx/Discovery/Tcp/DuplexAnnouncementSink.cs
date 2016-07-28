// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;

namespace ServiceModelEx.TcpDiscovery
{
   [CallbackBehavior(UseSynchronizationContext = false,ConcurrencyMode = ConcurrencyMode.Multiple)]
   public class DuplexAnnouncementSink<T> : AnnouncementSink<T>,IAnnouncements where T : class
   {
      IAnnouncementsSubscription m_SubscriptionProxy; 

      public override void Open()
      {
         base.Open();

         if(m_SubscriptionProxy == null)
         {
            m_SubscriptionProxy = DuplexChannelFactory<IAnnouncementsSubscription,IAnnouncements>.CreateChannel(this,DiscoveryFactory.Binding,DiscoveryFactory.Address.AnnouncementsSubscription);
            m_SubscriptionProxy.Subscribe();
         }
      }
      public override void Close()
      {
         if((m_SubscriptionProxy as ICommunicationObject).State == CommunicationState.Opened)
         {
            try
            {
               m_SubscriptionProxy.Unsubscribe();
               (m_SubscriptionProxy as ICommunicationObject).Close();
            }
            catch
            {}
            finally
            {
               m_SubscriptionProxy = null;
            }            
         }

         base.Close();
      }

      void IAnnouncements.OnHello(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         AnnouncementEventArgs args = DiscoveryHelper.CreateAnnouncementArgs(address,contractName,contractNamespace,scopes);
         OnHello(this,args);
      }

      void IAnnouncements.OnBye(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         AnnouncementEventArgs args = DiscoveryHelper.CreateAnnouncementArgs(address,contractName,contractNamespace,scopes);        
         OnBye(this,args);
      }
   }
}