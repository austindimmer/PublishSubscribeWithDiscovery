// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;


namespace ServiceModelEx.ServiceBus
{
   public partial class DiscoverableServiceHost
   {
      [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,UseSynchronizationContext = false)]
      class DiscoveryRequestService : IServiceBusDiscovery
      {
         readonly ServiceEndpoint[] Endpoints;
         NetOnewayRelayBinding m_DiscoveryResponseBinding;

         public NetOnewayRelayBinding DiscoveryResponseBinding
         {
            get
            {
               if(m_DiscoveryResponseBinding == null)
               {
                  m_DiscoveryResponseBinding = new NetOnewayRelayBinding();
               }
               return m_DiscoveryResponseBinding;
            }
            set
            {
               m_DiscoveryResponseBinding = value;
            }
         }

         TransportClientEndpointBehavior Credentials
         {
            get
            {
               TransportClientEndpointBehavior credentials = null;

               ServiceEndpointCollection endpoints = OperationContext.Current.Host.Description.Endpoints;
               foreach(ServiceEndpoint endpoint in endpoints)
               {
                  credentials = endpoint.Behaviors.Find<TransportClientEndpointBehavior>();
                  if(credentials != null)
                  {
                     break;
                  }
               }
               return credentials;           
            }
         }

         public DiscoveryRequestService(ServiceEndpoint[] endpoints)
         {
            Endpoints = endpoints;
         }

         void IServiceBusDiscovery.OnDiscoveryRequest(string contractName,string contractNamespace,Uri[] scopesToMatch,Uri responseAddress)
         {
            ChannelFactory<IServiceBusDiscoveryCallback> factory = new ChannelFactory<IServiceBusDiscoveryCallback>(DiscoveryResponseBinding,new EndpointAddress(responseAddress));
            factory.Endpoint.Behaviors.Add(Credentials);

            IServiceBusDiscoveryCallback callback = factory.CreateChannel();

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
                  try
                  {
                     callback.DiscoveryResponse(endpoint.Address.Uri,contractName,contractNamespace,scopes);
                  }
                  catch
                  {
                     callback = factory.CreateChannel();
                  }
               }
            }
            (callback as ICommunicationObject).Close();
         }
      }   
   }
}

