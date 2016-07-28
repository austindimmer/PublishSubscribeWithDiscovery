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

namespace ServiceModelEx
{
   public static class MetadataHelper
   {
      const int MessageSizeMultiplier = 5;

      static ServiceEndpointCollection QueryMexEndpoint(string mexAddress,BindingElement bindingElement)
      {
         dynamic element = bindingElement;
         element.MaxReceivedMessageSize *= MessageSizeMultiplier;

         CustomBinding binding = new CustomBinding(element);
         
         MetadataExchangeClient mexClient = new MetadataExchangeClient(binding);
         MetadataSet metadata = mexClient.GetMetadata(new EndpointAddress(mexAddress));
         MetadataImporter importer = new WsdlImporter(metadata);
         return importer.ImportAllEndpoints();
      }
      
      public static ServiceEndpoint[] GetEndpoints(string mexAddress,Type contractType)
      {
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);
         ContractDescription description = ContractDescription.GetContract(contractType);

         return endpoints.Where((endpoint) => endpoint.Contract.Name == description.Name && endpoint.Contract.Namespace == description.Namespace);       
      }
          
      public static ServiceEndpoint[] GetEndpoints(string mexAddress)
      {
         if(string.IsNullOrWhiteSpace(mexAddress))
         {
            throw new ArgumentException("mexAddress");
         }

         Uri address = new Uri(mexAddress);
         ServiceEndpointCollection endpoints = null;
         BindingElement bindingElement = null;

         //Try over HTTP-GET first
         if(address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps)
         {
            string getAddress = mexAddress;
            if(mexAddress.EndsWith("?wsdl") == false)
            {
               getAddress += "?wsdl";
            }
            if(address.Scheme == Uri.UriSchemeHttp)
            {
               HttpTransportBindingElement httpBindingElement = new HttpTransportBindingElement();
               httpBindingElement.MaxReceivedMessageSize *= MessageSizeMultiplier;
               bindingElement = httpBindingElement;
            }
            else
            {
               HttpsTransportBindingElement httpsBindingElement = new HttpsTransportBindingElement();
               httpsBindingElement.MaxReceivedMessageSize *= MessageSizeMultiplier;
               bindingElement = httpsBindingElement;
            }
            CustomBinding binding = new CustomBinding(bindingElement);

            MetadataExchangeClient mexClient = new MetadataExchangeClient(binding);
            MetadataSet metadata = mexClient.GetMetadata(new Uri(getAddress),MetadataExchangeClientMode.HttpGet);
            MetadataImporter importer = new WsdlImporter(metadata);
            endpoints = importer.ImportAllEndpoints();
            return endpoints.ToArray();
         }

         //Try MEX endpoint:

         if(address.Scheme == Uri.UriSchemeHttp)
         {
            bindingElement = new HttpTransportBindingElement();
         }
         if(address.Scheme == Uri.UriSchemeHttps)
         {
            bindingElement = new HttpsTransportBindingElement();
         }
         if(address.Scheme == Uri.UriSchemeNetTcp)
         {
            bindingElement = new TcpTransportBindingElement();
         }
         if(address.Scheme == Uri.UriSchemeNetPipe)
         {
            bindingElement = new NamedPipeTransportBindingElement();
         }
                     
         endpoints = QueryMexEndpoint(mexAddress,bindingElement);
         return endpoints.ToArray();
      }
      public static Type GetCallbackContract(string mexAddress,Type contractType)
      {
         if(contractType.IsInterface == false)
         {
            Debug.Assert(false,contractType + " is not an interface");
            return null;
         }

         object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute),false);
         if(attributes.Length == 0)
         {
            Debug.Assert(false,"Interface " + contractType + " does not have the ServiceContractAttribute");
            return null;
         }
         ContractDescription description = ContractDescription.GetContract(contractType);

         return GetCallbackContract(mexAddress,description.Namespace,description.Name);
      }

      public static Type GetCallbackContract(string mexAddress,string contractNamespace,string contractName)
      {
         if(string.IsNullOrWhiteSpace(mexAddress)) throw new ArgumentException("mexAddress");
         if(string.IsNullOrWhiteSpace(contractName)) throw new ArgumentException("contractName");
         if(String.IsNullOrEmpty(contractNamespace))
         {
            Debug.Assert(false,"Empty namespace");
            return null;
         }
         if(String.IsNullOrEmpty(contractName))
         {
            Debug.Assert(false,"Empty name");
            return null;
         }
         try
         {
            ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);
            foreach(ServiceEndpoint endpoint in endpoints)
            {
               if(endpoint.Contract.Namespace == contractNamespace && endpoint.Contract.Name == contractName)
               {
                  return endpoint.Contract.CallbackContractType;
               }
            }
         }
         catch
         {}
         return null;
      }
      
      public static bool QueryContract(string mexAddress,Type contractType)
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

         ContractDescription description = ContractDescription.GetContract(contractType);

         return QueryContract(mexAddress,description.Namespace,description.Name);
      }
      public static bool QueryContract(string mexAddress,string contractNamespace,string contractName)
      {
         // Check mex address and contract namespace.
         if(string.IsNullOrWhiteSpace(mexAddress)) throw new ArgumentException("mexAddress");
         if(string.IsNullOrWhiteSpace(contractNamespace)) throw new ArgumentException("contractNamespace");
         if(string.IsNullOrWhiteSpace(contractName)) throw new ArgumentException("contractName");
         
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);
         return endpoints.Any(endpoint => endpoint.Contract.Namespace == contractNamespace && endpoint.Contract.Name == contractName);
      }
      public static ContractDescription[] GetContracts(string mexAddress)
      {
         return GetContracts(typeof(Binding),mexAddress);
      }
      public static ContractDescription[] GetContracts(Type bindingType,string mexAddress)
      {
         Debug.Assert(bindingType.IsSubclassOf(typeof(Binding)) || bindingType == typeof(Binding));
         
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);

         List<ContractDescription> contracts = new List<ContractDescription>();
         ContractDescription contract;
         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(bindingType.IsInstanceOfType(endpoint.Binding))
            {
               contract = new ContractDescription(endpoint.Contract.Name,endpoint.Contract.Namespace);

               if(contracts.Any((item)=> item.Name == contract.Name && item.Namespace == contract.Namespace) == false)
               {
                  contracts.Add(contract);
               }
            }
         }
         return contracts.ToArray();
      }
      public static string[] GetAddresses(string mexAddress,Type contractType) 
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

         return GetAddresses(mexAddress,description.Namespace,description.Name);
      }
      public static string[] GetAddresses(string mexAddress,string contractNamespace,string contractName)
      {
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);

         List<string> addresses = new List<string>();

         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(endpoint.Contract.Namespace == contractNamespace && endpoint.Contract.Name == contractName)
            {
               Debug.Assert(addresses.Contains(endpoint.Address.Uri.AbsoluteUri) == false);
               addresses.Add(endpoint.Address.Uri.AbsoluteUri);
            }
         }
         return addresses.ToArray();
      }
      public static string[] GetAddresses(Type bindingType,string mexAddress,Type contractType)
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
         ContractDescription description = ContractDescription.GetContract(contractType);

         return GetAddresses(bindingType,mexAddress,description.Namespace,description.Name);
      }
      public static string[] GetAddresses(Type bindingType,string mexAddress,string contractNamespace,string contractName)
      {
         Debug.Assert(bindingType.IsSubclassOf(typeof(Binding)) || bindingType == typeof(Binding));

         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);

         List<string> addresses = new List<string>();

         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(bindingType.IsInstanceOfType(endpoint.Binding))
            {
               if(endpoint.Contract.Namespace == contractNamespace && endpoint.Contract.Name == contractName) 
               {
                  Debug.Assert(addresses.Contains(endpoint.Address.Uri.AbsoluteUri) == false);
                  addresses.Add(endpoint.Address.Uri.AbsoluteUri);
               }
            }
         }
         return addresses.ToArray();
      }
      public static string[] GetOperations(string mexAddress,Type contractType)
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

         return GetOperations(mexAddress,description.Namespace,description.Name);
      }
      public static string[] GetOperations(string mexAddress,string contractNamespace,string contractName)
      {
         ServiceEndpoint[] endpoints = GetEndpoints(mexAddress);

         List<string> operations = new List<string>();

         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(endpoint.Contract.Namespace == contractNamespace && endpoint.Contract.Name == contractName)
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
