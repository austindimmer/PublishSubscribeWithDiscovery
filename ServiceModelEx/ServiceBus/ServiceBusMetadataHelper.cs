// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public static class ServiceBusMetadataHelper
   {
      const int MessageSizeMultiplier = 5;

      static ServiceEndpointCollection QueryMexEndpoint(string mexAddress,Binding binding,TokenProvider tokenProvider)
      {
         dynamic extendedBinding = binding;
         extendedBinding.MaxReceivedMessageSize *= MessageSizeMultiplier;

         MetadataExchangeClient mexClient = new MetadataExchangeClient(extendedBinding);
         mexClient.SetServiceBusCredentials(tokenProvider);
         MetadataSet metadata = mexClient.GetMetadata(new EndpointAddress(mexAddress));
         MetadataImporter importer = new WsdlImporter(metadata);
         return importer.ImportAllEndpoints();
      }

      public static ServiceEndpoint[] GetEndpoints(string mexAddress,string secret)
      {
         return GetEndpoints(mexAddress,ServiceBusHelper.DefaultIssuer,secret);
      }
      public static ServiceEndpoint[] GetEndpoints(string mexAddress,string issuer,string secret)
      {
         return GetEndpoints(mexAddress,TokenProvider.CreateSharedSecretTokenProvider(issuer,secret));
      }
      public static ServiceEndpoint[] GetEndpoints(string mexAddress,TokenProvider tokenProvider)
      {
         if(string.IsNullOrWhiteSpace(mexAddress))
         {
            throw new ArgumentException("mexAddress");
         }

         Uri address = new Uri(mexAddress);
    
         ServiceEndpointCollection endpoints = null;
         
         Binding binding;

         if(address.Scheme == "sb")
         {
            binding = new NetTcpRelayBinding();
         }
         else
         {
            Debug.Assert(address.Scheme == "http" || address.Scheme == "https");
            binding = new WS2007HttpRelayBinding();
         }

         try
         {
            endpoints = QueryMexEndpoint(mexAddress,binding,tokenProvider);
         }
         catch
         {}
         if(endpoints != null)
         {
            return endpoints.ToArray();
         }
         else
         {
            return new ServiceEndpoint[]{};
         }
      }
      
      public static ServiceEndpoint[] GetEndpoints(string mexAddress,Type contractType,string secret)
      {
         return GetEndpoints(mexAddress,contractType,ServiceBusHelper.DefaultIssuer,secret); 
      }
      public static ServiceEndpoint[] GetEndpoints(string mexAddress,Type contractType,string issuer,string secret)
      {
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress,issuer,secret);
         ContractDescription description = ContractDescription.GetContract(contractType);
         return endpoints.Where((endpoint) => endpoint.Contract.Name == description.Name && endpoint.Contract.Namespace == description.Namespace);       
      }     
      
      public static bool QueryContract(string mexAddress,Type contractType,string secret)
      {
         return QueryContract(mexAddress,contractType,ServiceBusHelper.DefaultIssuer,secret); 
      }
      public static bool QueryContract(string mexAddress,Type contractType,string issuer,string secret)
      {
         if(contractType.IsInterface == false)
         {
            Debug.Assert(false,contractType + " is not an interface");
            return false;
         }

         object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute),false);
         if(attributes.Length == 0)
         {
            Debug.Assert(false,"Interface " + contractType + " does not have the ServiceContractAttribute");
            return false;
         }

         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress,issuer,secret);
            
         ContractDescription description = ContractDescription.GetContract(contractType);
            
         return endpoints.Any(endpoint => endpoint.Contract.Name == description.Name && endpoint.Contract.Namespace == description.Namespace);
      }
      
      public static ContractDescription[] GetContracts(string mexAddress,string secret)
      {
         return GetContracts(mexAddress,ServiceBusHelper.DefaultIssuer,secret); 
      }
      public static ContractDescription[] GetContracts(string mexAddress,string issuer,string secret)
      {
         return GetContracts(typeof(Binding),mexAddress,issuer,secret);
      }
      
      public static ContractDescription[] GetContracts(Type bindingType,string mexAddress,string secret)
      {
         return GetContracts(bindingType,mexAddress,ServiceBusHelper.DefaultIssuer,secret); 
      }
      public static ContractDescription[] GetContracts(Type bindingType,string mexAddress,string issuer,string secret)
      {
         Debug.Assert(bindingType.IsSubclassOf(typeof(Binding)) || bindingType == typeof(Binding));
         
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress,issuer,secret);

         List<ContractDescription> contracts = new List<ContractDescription>();
         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(bindingType.IsInstanceOfType(endpoint.Binding))
            { 
               if(contracts.Any((item)=> item.Name == endpoint.Contract.Name && item.Namespace == endpoint.Contract.Namespace) == false)
               {
                  contracts.Add(endpoint.Contract);
               }
            }
         }
         return contracts.ToArray();
      }
      
      public static string[] GetAddresses(string mexAddress,Type contractType,string secret) 
      {
         return GetAddresses(mexAddress,contractType,ServiceBusHelper.DefaultIssuer,secret); 
      }
      public static string[] GetAddresses(string mexAddress,Type contractType,string issuer,string secret) 
      {
         if(contractType.IsInterface == false)
         {
            Debug.Assert(false,contractType + " is not an interface");
            return new string[]{};
         }

         object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute),false);
         if(attributes.Length == 0)
         {
            Debug.Assert(false,"Interface " + contractType + " does not have the ServiceContractAttribute");
            return new string[]{};
         }
         
         ContractDescription description = ContractDescription.GetContract(contractType);
         return GetAddresses(mexAddress,description.Namespace,description.Name,issuer,secret);
      }
      public static string[] GetAddresses(string mexAddress,string contractNamespace,string contractName,string secret) 
      {
         return GetAddresses(mexAddress,contractNamespace,contractName,ServiceBusHelper.DefaultIssuer,secret); 
      }
      public static string[] GetAddresses(string mexAddress,string contractNamespace,string contractName,string issuer,string secret) 
      {
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress,issuer,secret);

         List<string> addresses = new List<string>();

         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(endpoint.Contract.Name == contractName && endpoint.Contract.Namespace == contractNamespace)
            {
               Debug.Assert(addresses.Contains(endpoint.Address.Uri.AbsoluteUri) == false);
               addresses.Add(endpoint.Address.Uri.AbsoluteUri);
            }
         }
         return addresses.ToArray();
      }






      public static string[] GetAddresses(Type bindingType,string mexAddress,Type contractType,string secret)
      {
         return GetAddresses(bindingType,mexAddress,contractType,ServiceBusHelper.DefaultIssuer,secret);
      }
      public static string[] GetAddresses(Type bindingType,string mexAddress,Type contractType,string issuer,string secret)
      {
         Debug.Assert(bindingType.IsSubclassOf(typeof(Binding)) || bindingType == typeof(Binding));

         if(contractType.IsInterface == false)
         {
            Debug.Assert(false,contractType + " is not an interface");
            return new string[]{};
         }

         object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute),false);
         if(attributes.Length == 0)
         {
            Debug.Assert(false,"Interface " + contractType + " does not have the ServiceContractAttribute");
            return new string[]{};
         }

         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress,issuer,secret);

         List<string> addresses = new List<string>();

         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(bindingType.IsInstanceOfType(endpoint.Binding))
            {
               ContractDescription description = ContractDescription.GetContract(contractType);
               if(endpoint.Contract.Name == description.Name && endpoint.Contract.Namespace == description.Namespace)
               {
                  Debug.Assert(addresses.Contains(endpoint.Address.Uri.AbsoluteUri) == false);
                  addresses.Add(endpoint.Address.Uri.AbsoluteUri);
               }
            }
         }
         return addresses.ToArray();
      }
  
      public static string[] GetOperations(string mexAddress,Type contractType,string secret)
      {
         return GetOperations(mexAddress,contractType,ServiceBusHelper.DefaultIssuer,secret);
      }

      public static string[] GetOperations(string mexAddress,Type contractType,string issuer,string secret)
      {
         if(contractType.IsInterface == false)
         {
            Debug.Assert(false,contractType + " is not an interface");
            return new string[]{};
         }

         object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute),false);
         if(attributes.Length == 0)
         {
            Debug.Assert(false,"Interface " + contractType + " does not have the ServiceContractAttribute");
            return new string[]{};
         }
         ContractDescription description = ContractDescription.GetContract(contractType);

         return GetOperations(mexAddress,description.Namespace,description.Name,issuer,secret);
      }

      public static string[] GetOperations(string mexAddress,string contractNamespace,string contractName,string secret)
      {
         return GetOperations(mexAddress,contractNamespace,contractName,ServiceBusHelper.DefaultIssuer,secret);
      }
      public static string[] GetOperations(string mexAddress,string contractNamespace,string contractName,string issuer,string secret)
      {      
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress,issuer,secret);

         List<string> operations = new List<string>();

         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(endpoint.Contract.Name == contractName && endpoint.Contract.Namespace == contractNamespace)
            {
               foreach(OperationDescription operation in endpoint.Contract.Operations)
               {
                  Debug.Assert(operations.Contains(operation.Name) == false);
                  operations.Add(operation.Name);
               }
               break;
            }
         }
         return operations.ToArray();
      }
   }
}
