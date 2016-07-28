// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModelEx.TcpDiscovery
{
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,UseSynchronizationContext = false,ConcurrencyMode = ConcurrencyMode.Multiple)]
   public class DiscoveryService : IDiscovery,IDiscoverySubscription,IAnnouncementsSubscription,IAnnouncements
   {
      List<string> m_Addresses;
      List<IAnnouncements> m_NotifiedClients;

      public DiscoveryService()
      {
         m_Addresses = new List<string>();
         m_NotifiedClients = new List<IAnnouncements>();
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      void IDiscovery.OnDiscoveryRequest(string contractName,string contractNamespace,Uri[] scopesToMatch)
      {
         if(m_Addresses.Count == 0)
         {
            return;
         }
         //Callback to the client wanting to discover
         IDiscoveryCallback clientCallback = OperationContext.Current.GetCallbackChannel<IDiscoveryCallback>();                                    
         DiscoveryCallback serviceCallback = new DiscoveryCallback(clientCallback);

         Action<string> discover = (address)=>
                                   {

                                      IDiscovery serviceProxy = DuplexChannelFactory<IDiscovery,IDiscoveryCallback>.CreateChannel(serviceCallback,DiscoveryFactory.Binding,new EndpointAddress(address as string));

                                      CancellationTokenSource cancellationSource = new CancellationTokenSource();
                                      EventHandler cleanup =  delegate
                                                              {
                                                                 //Will still be Opening if service was not found and the discovery period expires.
                                                                 ICommunicationObject proxy = serviceProxy as ICommunicationObject;
                                                                 if(proxy.State != CommunicationState.Faulted)
                                                                 {
                                                                    try
                                                                    {
                                                                       proxy.Close();
                                                                    }
                                                                    catch
                                                                    {}
                                                                 }
                                                                 cancellationSource.Cancel();
                                                              };
                                      (clientCallback as ICommunicationObject).Closed  += cleanup;
                                      (clientCallback as ICommunicationObject).Faulted += cleanup;
                                      Task.Delay(DiscoveryFactory.Binding.SendTimeout).ContinueWith(_=>cleanup(null,EventArgs.Empty),cancellationSource.Token);

                                      try
                                      {
                                         serviceProxy.OnDiscoveryRequest(contractName,contractNamespace,scopesToMatch);
                                      }
                                      catch
                                      {
                                         Trace.Write("Some problem occurred publishing to a service.");
                                      }
                                   };

         m_Addresses.ForEachAsync(discover);
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      void IDiscoverySubscription.Subscribe(string address)
      {
         if(m_Addresses.Contains(address) == false)
         {
            m_Addresses.Add(address);
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      void IDiscoverySubscription.Unsubscribe(string address)
      {
         if(m_Addresses.Contains(address) == true)
         {
            m_Addresses.Remove(address);
         }
      }
      
      [CallbackBehavior(UseSynchronizationContext = false,ConcurrencyMode = ConcurrencyMode.Multiple)]
      class DiscoveryCallback : IDiscoveryCallback
      {
         IDiscoveryCallback m_DiscoveryCallback;

         public DiscoveryCallback(IDiscoveryCallback discoveryCallback)
         {
            m_DiscoveryCallback = discoveryCallback;
         }
         public void OnDiscoveryResponse(Uri address,string contractName,string contractNamespace,Uri[] scopes)
         {
            try
            {
               m_DiscoveryCallback.OnDiscoveryResponse(address,contractName,contractNamespace,scopes);
            }
            catch
            {}
         }
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      void IAnnouncementsSubscription.Subscribe()
      {
         IAnnouncements client = OperationContext.Current.GetCallbackChannel<IAnnouncements>();
         if(m_NotifiedClients.Contains(client) == false)
         {
            m_NotifiedClients.Add(client);
         }
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      void IAnnouncementsSubscription.Unsubscribe()
      {
         IAnnouncements client = OperationContext.Current.GetCallbackChannel<IAnnouncements>();
         if(m_NotifiedClients.Contains(client))
         {
            m_NotifiedClients.Remove(client);
         }
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      void IAnnouncements.OnHello(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         Action<IAnnouncements> hello = (client)=>
                                        {
                                           try
                                           {
                                              client.OnHello(address,contractName,contractNamespace,scopes);
                                           }
                                           catch
                                           {
                                              Trace.WriteLine("Some error trying to announce Hello");
                                           }
                                        };
         m_NotifiedClients.ForEachAsync(hello);
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      void IAnnouncements.OnBye(Uri address,string contractName,string contractNamespace,Uri[] scopes)
      {
         Action<IAnnouncements> bye = (client)=>
                                      {
                                         try
                                         {
                                            client.OnBye(address,contractName,contractNamespace,scopes);
                                         }
                                         catch
                                         {
                                            Trace.WriteLine("Some error trying to announce Bye");
                                         }
                                      };       
         m_NotifiedClients.ForEachAsync(bye);
      }
   }
}