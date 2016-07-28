   // © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.ServiceBus;
using System.IO;

namespace ServiceModelEx.ServiceBus
{
   public class ServiceBusEventsHost : ServiceBusHost
   {
      NetOnewayRelayBinding m_RelayBinding;

      //Managing the subscriptions
      Dictionary<string,List<string>> Subscriptions
      {get;set;}

      public virtual NetOnewayRelayBinding RelayBinding
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            if(m_RelayBinding == null)
            {
               RelayBinding = new NetEventRelayBinding();
            }
            return m_RelayBinding;
         }
         [MethodImpl(MethodImplOptions.Synchronized)]
         set
         {
            Debug.Assert(value is NetEventRelayBinding);

            m_RelayBinding = value;
         }
      }

      public ServiceBusEventsHost(object singletonInstance,Uri baseAddress) : this(singletonInstance,new Uri[]{baseAddress})
      {}

      public ServiceBusEventsHost(object singletonInstance,Uri[] baseAddresses) : base(singletonInstance,baseAddresses)
      {
         Initialize();
      }
      public ServiceBusEventsHost(Type serviceType,Uri baseAddress) : this(serviceType,new Uri[]{baseAddress})
      {}

      public ServiceBusEventsHost(Type serviceType,Uri[] baseAddresses) : base(serviceType,baseAddresses)
      {
          Initialize();
      }
      void Initialize()
      {
         Subscriptions = new Dictionary<string,List<string>>();        
         
         foreach(Uri baseAddress in BaseAddresses)
         {
            if(baseAddress.Scheme != "sb")
            {
               throw new InvalidOperationException("Can only use 'sb' for base address scheme");
            }

            Type[] contracts = ServiceBusHelper.GetServiceContracts(Description.ServiceType);
            foreach(Type contract in contracts)
            {
               AddServiceEndpoint(contract,RelayBinding,baseAddress.AbsoluteUri + contract + "/");
               Subscriptions[contract.Name] = new List<string>();
            }
         }

         IEndpointBehavior selector = new EventSelector(Subscriptions);

         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {
            endpoint.Behaviors.Add(selector); 
         }
      }
        
      [MethodImpl(MethodImplOptions.Synchronized)]
      public void SetBinding(string bindingConfigName)
      {
         RelayBinding = new NetEventRelayBinding(bindingConfigName);
      }

      public void Subscribe()
      {
         Type[] contracts = ServiceBusHelper.GetServiceContracts(Description.ServiceType);
         lock(Subscriptions)
         {
            foreach(Type contract in contracts)
            {
               Subscribe(contract);
            }
         }
      }

      public void Subscribe(Type contractType)
      {
         string[] operations = GetOperations(contractType);

         lock(Subscriptions)
         {
            foreach(string operationName in operations)
            {
               Subscribe(contractType,operationName);
            }
         }
      }
       
      public void Subscribe(Type contractType,string operation)
      {    
         VerifyTypeOperation(Description.ServiceType,contractType,operation);

         lock(Subscriptions)
         {
            if(Subscriptions[contractType.Name].Contains(operation) == false)
            {
               Subscriptions[contractType.Name].Add(operation);
            }
        }
      }

      public void Unsubscribe()
      {
         lock(Subscriptions)
         {
            Type[] contracts = ServiceBusHelper.GetServiceContracts(Description.ServiceType);

            foreach(Type contract in contracts)
            {
               Unsubscribe(contract);
            }
         }
      }
            
      public void Unsubscribe(Type contractType)
      {
         string[] operations = GetOperations(contractType);

         lock(Subscriptions)
         {
            foreach(string operationName in operations)
            {
               Unsubscribe(contractType,operationName);
            }
         }
      }   
     
      public void Unsubscribe(Type contractType,string operation)
      {
         VerifyTypeOperation(Description.ServiceType,contractType,operation);

         Debug.Assert(String.IsNullOrEmpty(operation) == false);

         lock(Subscriptions)
         {
            if(Subscriptions[contractType.Name].Contains(operation))
            {
               Subscriptions[contractType.Name].Remove(operation);
            }
         }
      }
      static internal string[] GetOperations(Type contract)
      {
         MethodInfo[] methods = contract.GetMethods(BindingFlags.Public|BindingFlags.FlattenHierarchy|BindingFlags.Instance);
         List<string> operations = new List<string>(methods.Length);

         Action<MethodInfo> add = (method)=>
                                  {
                                     Debug.Assert(! operations.Contains(method.Name));
                                     operations.Add(method.Name);
                                  };
         methods.ForEach(add);
         return operations.ToArray();
      }
      
      static void VerifyTypeOperation(Type serviceType,Type contractType,string operation)
      {
         if(String.IsNullOrEmpty(operation) || String.IsNullOrWhiteSpace(operation))
         {
            throw new InvalidOperationException("Operation: " + operation + " cannot be null or empty");
         }

         Type[] contracts = ServiceBusHelper.GetServiceContracts(serviceType);
         if(contracts.Contains(contractType) == false)
         {
            throw new InvalidOperationException("Service type: " + serviceType.FullName + " does not support contrat: " + contractType.FullName);
         }

         if(GetOperations(contractType).Contains(operation) == false)
         {
            throw new InvalidOperationException("Contract type: " + contractType.FullName + " does not support operation: " + operation);
         }
      }

      override protected Uri[] Addresses
      {
         get
         {
            List<Uri> addresses = new List<Uri>();

            lock(Subscriptions)
            {
               foreach(Uri baseAddress in BaseAddresses)
               {
                  foreach(string contract in Subscriptions.Keys)
                  {
                     List<string> events = Subscriptions[contract];
                     foreach(string operation in events)
                     {
                        addresses.Add(new Uri(baseAddress.AbsoluteUri + contract + "/" + operation +"/"));
                     }
                  }
               }
            }
            return addresses.ToArray();
         }
      }
   
      class EventSelector : IDispatchOperationSelector,IEndpointBehavior
      {
         readonly Dictionary<string,List<string>> m_Subscriptions;

         public EventSelector(Dictionary<string,List<string>> subscriptions)
         {
            Debug.Assert(subscriptions != null);
            Debug.Assert(subscriptions.Keys.Any());

            m_Subscriptions = subscriptions;
         }
         public string SelectOperation(ref Message message)
         {
            string[] slashes = message.Headers.Action.Split('/');
            string contract = slashes[slashes.Length-2];
            string operation = slashes[slashes.Length-1];

            lock(m_Subscriptions)
            {
               if(m_Subscriptions[contract].Contains(operation))
               {
                  return operation;
               }
               else
               {
                  return null;
               }
            }
         }

         void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint,EndpointDispatcher endpointDispatcher)
         {
            endpointDispatcher.DispatchRuntime.OperationSelector = this;
         }

         void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint,BindingParameterCollection bindingParameters)
         {}
         void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint,ClientRuntime clientRuntime)
         {}
         void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
         {}
      }
   }
}




