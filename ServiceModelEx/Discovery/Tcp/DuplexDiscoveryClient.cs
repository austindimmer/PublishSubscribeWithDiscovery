// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;

namespace ServiceModelEx.TcpDiscovery
{
   public class DuplexDiscoveryClient
   {
      public DuplexDiscoveryClient()
      {}

      public DuplexDiscoveryClient(string endpointName) 
      {
          DiscoveryFactory.Address.SetDiscoveryService(endpointName);
      }
      public DuplexDiscoveryClient(EndpointAddress address) 
      {
         DiscoveryFactory.Address.SetDiscoveryService(address);
      }

      public FindResponse Find(FindCriteria criteria)
      {
         string contractName = criteria.ContractTypeNames[0].Name;
         string contractNamespace = criteria.ContractTypeNames[0].Namespace;

         FindResponse response = DiscoveryHelper.CreateFindResponse();

         ManualResetEvent handle = new ManualResetEvent(false);

         Action<Uri,Uri[]> addEndpoint = (address,scopes)=>
                                         {
                                            if(handle.WaitOne(0))
                                            {
                                               return;
                                            }
                                            EndpointDiscoveryMetadata metadata = new EndpointDiscoveryMetadata();
                                            metadata.Address = new EndpointAddress(address);
                                            if(scopes != null)
                                            {
                                               foreach(Uri scope in scopes)
                                               {
                                                  metadata.Scopes.Add(scope); 
                                               }      
                                            }
                                            lock(response)
                                            {
                                               response.Endpoints.Add(metadata);

                                               if(response.Endpoints.Count >= criteria.MaxResults)
                                               {
                                                  handle.Set();
                                               }
                                            }
                                         };

         //To receive the callbacks from the services
         ClientDiscoveryResponseCallback callback = new ClientDiscoveryResponseCallback(contractName,contractNamespace,addEndpoint);

         IDiscovery discoveryServiceProxy = DuplexChannelFactory<IDiscovery,IDiscoveryCallback>.CreateChannel(callback,DiscoveryFactory.Binding,DiscoveryFactory.Address.DiscoveryService);

         discoveryServiceProxy.OnDiscoveryRequest(criteria.ContractTypeNames[0].Name,criteria.ContractTypeNames[0].Namespace,criteria.Scopes.ToArray());

         bool found = handle.WaitOne(criteria.Duration);
         handle.Set();
         handle.Close();

         //If there is cardinality of 'some' the channel will fault, but we don't care here as we already got the max results
         try
         {
            (discoveryServiceProxy as ICommunicationObject).Close();
         }
         catch
         {}

         if(found == false)
         {
            Trace.WriteLine("Could not find endpoints within specified discovery criteria duration");
         }
    
         return response;
      }
      
      [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,UseSynchronizationContext = false)]
      class ClientDiscoveryResponseCallback : IDiscoveryCallback
      {
         readonly string ContractNamespace;
         readonly string ContractName;
         readonly Action<Uri,Uri[]> Action;

         public ClientDiscoveryResponseCallback(string contractName,string contractNamespace,Action<Uri,Uri[]> action)
         {
            ContractName = contractName;
            ContractNamespace = contractNamespace;
            Action = action;
         }
         public void OnDiscoveryResponse(Uri address,string contractName,string contractNamespace,Uri[] scopes)
         {
            if(ContractName != contractName)
            {
               throw new InvalidOperationException("Unexpected contract name in discovery response. Expected was " + ContractName + " but the response was " + contractName);
            }
            if(ContractNamespace != contractNamespace)
            {
               throw new InvalidOperationException("Unexpected contract namespace in discovery response. Expected was " + ContractNamespace + " but the response was " + contractNamespace);
            }
            Action(address,scopes);
         }
      }
   }
}


