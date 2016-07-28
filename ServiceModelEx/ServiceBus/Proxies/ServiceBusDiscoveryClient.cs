// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using Microsoft.ServiceBus;


namespace ServiceModelEx.ServiceBus
{
   public class ServiceBusDiscoveryClient : ClientBase<IServiceBusDiscovery>,IServiceBusProperties
   {         
      protected Uri ResponseAddress
      {get;private set;}

      public ServiceBusDiscoveryClient(string serviceNamespace,string secret) : this(new NetOnewayRelayBinding(),new EndpointAddress(ServiceBusEnvironment.CreateServiceUri("sb",serviceNamespace,DiscoverableServiceHost.DiscoveryPath)))
      {
         this.SetServiceBusCredentials(secret);
      }
      public ServiceBusDiscoveryClient(string endpointName) : base(endpointName)
      {
         Initialize();
      }
      public ServiceBusDiscoveryClient(NetOnewayRelayBinding binding,EndpointAddress address) : base(binding,address)
      {
         Initialize();
      }
      void Initialize()
      {
         Debug.Assert(Endpoint.Address.Uri.Scheme == "sb");
         Debug.Assert(Endpoint.Binding is NetOnewayRelayBinding);

         string serviceNamespace = ServiceBusHelper.ExtractNamespace(Endpoint.Address.Uri);
         ResponseAddress = ServiceBusEnvironment.CreateServiceUri("sb",serviceNamespace,"DiscoveryResponses/" + Guid.NewGuid());
      }
      public FindResponse Find(FindCriteria criteria)
      {
         string contractName = criteria.ContractTypeNames[0].Name;
         string contractNamespace = criteria.ContractTypeNames[0].Namespace;

         FindResponse response = DiscoveryHelper.CreateFindResponse();

         ManualResetEvent handle = new ManualResetEvent(false);

         Action<Uri,Uri[]> addEndpoint = (address,scopes)=>
                                         {
                                            EndpointDiscoveryMetadata metadata = new EndpointDiscoveryMetadata();
                                            metadata.Address = new EndpointAddress(address);
                                            if(scopes != null)
                                            {
                                               foreach(Uri scope in scopes)
                                               {
                                                  metadata.Scopes.Add(scope);
                                               }
                                            }
                                            response.Endpoints.Add(metadata);
                                            
                                            if(response.Endpoints.Count >= criteria.MaxResults)
                                            {
                                               handle.Set();
                                            }
                                         };

         DiscoveryResponseCallback callback = new DiscoveryResponseCallback(contractName,contractNamespace,addEndpoint);

         ServiceHost host = new ServiceHost(callback);
         host.AddServiceEndpoint(typeof(IServiceBusDiscoveryCallback),Endpoint.Binding,ResponseAddress.AbsoluteUri);
         host.Description.Endpoints[0].Behaviors.Add(new ServiceRegistrySettings(DiscoveryType.Public));
         TransportClientEndpointBehavior credentials = Endpoint.Behaviors.Find<TransportClientEndpointBehavior>();
         Debug.Assert(credentials != null);

         host.Description.Endpoints[0].Behaviors.Add(credentials);

         host.Open();

         try
         {
            DiscoveryRequest(criteria.ContractTypeNames[0].Name,criteria.ContractTypeNames[0].Namespace,criteria.Scopes.ToArray(),ResponseAddress);

            bool found = handle.WaitOne(criteria.Duration);
            if(found == false)
            {
               Trace.WriteLine("Could not find endpoints within specified discovery criteria duration");
            }
         }
         catch
         {}
         finally
         {
            try
            {
               host.Abort();
            }
            catch(ProtocolException)
            {}
         }
         return response;
      }
      void DiscoveryRequest(string contractName,string contractNamespace,Uri[] scopesToMatch,Uri responseAddress)
      {
         Channel.OnDiscoveryRequest(contractName,contractNamespace,scopesToMatch,responseAddress);
      }

      [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,UseSynchronizationContext = false)]
      class DiscoveryResponseCallback : IServiceBusDiscoveryCallback
      {
         readonly string ContractNamespace;
         readonly string ContractName;
         readonly Action<Uri,Uri[]> Action;

         public DiscoveryResponseCallback(string contractName,string contractNamespace,Action<Uri,Uri[]> action)
         {
            ContractName = contractName;
            ContractNamespace = contractNamespace;
            Action = action;
         }
         public void DiscoveryResponse(Uri address,string contractName,string contractNamespace,Uri[] scopes)
         {
            if(ContractName != contractName)
            {
               throw new InvalidOperationException("Unexpected contract name in service bus discovery response. Expected was " + ContractName + " but the response was " + contractName);
            }
            if(ContractNamespace != contractNamespace)
            {
               throw new InvalidOperationException("Unexpected contract namespace in service bus discovery response. Expected was " + ContractNamespace + " but the response was " + contractNamespace);
            }
            Action(address,scopes);
         }
      }
      TransportClientEndpointBehavior IServiceBusProperties.Credential
      {
         get
         {
            return Endpoint.Behaviors.Find<TransportClientEndpointBehavior>();
         }
         set
         {
            Debug.Assert(Endpoint.Behaviors.Contains(typeof(TransportClientEndpointBehavior)) == false);
            Endpoint.Behaviors.Add(value);
         }
      }

      Uri[] IServiceBusProperties.Addresses
      {
         get
         {
            return new Uri[]{Endpoint.Address.Uri};
         }
      }
   }
}


