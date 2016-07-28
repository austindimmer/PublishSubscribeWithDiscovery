// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceModelEx.ServiceBus
{
   public static partial class ServiceBusHelper
   {
      static void SetBehavior(IEnumerable<ServiceEndpoint> endpoints,TransportClientEndpointBehavior credential)
      {
         foreach(ServiceEndpoint endpoint in endpoints)
         {
            if(endpoint.Binding is NetTcpRelayBinding         ||
               endpoint.Binding is WSHttpRelayBinding         ||
               endpoint.Binding is NetOnewayRelayBinding      ||
               endpoint.Binding is NetMessagingBinding)
            {
               if(endpoint.Behaviors.Contains(credential) == false)
               {
                  endpoint.Behaviors.Add(credential);
                  //break;
               }
            }
            else
            {
               Debug.Assert(endpoint.Address.Uri.AbsoluteUri.Contains("servicebus.windows.net") == false);
            }
         }
      }
      internal static void ConfigureBinding(Binding binding,bool anonymous = true)
      {
         if(binding is NetTcpRelayBinding)
         {
            NetTcpRelayBinding tcpBinding = (NetTcpRelayBinding)binding;
            tcpBinding.Security.Mode  = EndToEndSecurityMode.Message;
            if(anonymous)
            {
               tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            }
            else
            {
               tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            }

            tcpBinding.ConnectionMode = TcpRelayConnectionMode.Hybrid;
            tcpBinding.ReliableSession.Enabled = true; 

            return;
         }
         if(binding is WSHttpRelayBinding)
         {
            WSHttpRelayBinding wsBinding = (WSHttpRelayBinding)binding;
            wsBinding.Security.Mode = EndToEndSecurityMode.Message;
            if(anonymous)
            {
               wsBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            }
            else
            {
               wsBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            }
            wsBinding.ReliableSession.Enabled = true; 

            return;
         }
         if(binding is NetOnewayRelayBinding)
         {
            NetOnewayRelayBinding onewayBinding = (NetOnewayRelayBinding)binding;
            onewayBinding.Security.Mode = EndToEndSecurityMode.Message;
            if(anonymous)
            {
               onewayBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            }
            else
            {
               onewayBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            }
            return;
         }
         throw new InvalidOperationException(binding.GetType() + " is unsupported");
      }

      public static string ExtractNamespace(Uri address)
      {
         return address.Host.Split('.')[0];
      }

      public static string ExtractNamespace(string endpointName)
      {
         Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ServiceModelSectionGroup sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);

         foreach(ChannelEndpointElement endpointElement in sectionGroup.Client.Endpoints)
         {
            if(endpointElement.Name == endpointName)
            {
               return ExtractNamespace(endpointElement.Address);
            }
         }
         return null;
      }
      public static string ExtractNamespace(Type serviceType)
      {         
         Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ServiceModelSectionGroup sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);

         foreach(ServiceElement serviceElement in sectionGroup.Services.Services)
         {
            if(serviceElement.Name == serviceType.ToString())
            {
               return ExtractNamespace(serviceElement.Endpoints[0].Address);
            }
         }
         return null;
      }

      internal static Type[] GetServiceContracts(Type serviceType)
      {
         Debug.Assert(serviceType.IsClass);

         Type[] interfaces = serviceType.GetInterfaces();
         List<Type> contracts = new List<Type>();

         foreach(Type interfaceType in interfaces)
         {
            if(interfaceType.GetCustomAttributes(typeof(ServiceContractAttribute),false).Length == 1)
            {
               contracts.Add(interfaceType);
            }
         }
         return contracts.ToArray();
      }
      
      public static void AddServiceBusDefaultEndpoints(this ServiceHost host)
      {
         AddServiceBusDefaultEndpoints(host,host.BaseAddresses.ToArray());
      }
      internal static void AddServiceBusDefaultEndpoints(this ServiceHost host,Uri[] baseAddresses)
      {
         Debug.Assert(baseAddresses.Any(address=>address.Scheme == "sb"));

         Type[] contracts = GetServiceContracts(host.Description.ServiceType);
         Binding binding = new NetTcpRelayBinding();

         foreach(Uri baseAddress in baseAddresses)
         {
            if(baseAddress.Scheme != "sb")
            {
               continue;
            }           

            foreach(Type contract in contracts)
            {
               string address = baseAddress.AbsoluteUri;

               if(address.EndsWith("/") == false)
               {
                  address += "/";
               }  
               address += contract.Name;
               host.AddServiceEndpoint(contract,binding,address);
            }
         }
      }
   }
}





