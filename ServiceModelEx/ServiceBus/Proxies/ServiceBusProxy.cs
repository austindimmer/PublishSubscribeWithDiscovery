// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public abstract class ServiceBusClientBase<T> : ClientBase<T>,IServiceBusProperties where T : class
   {
      public ServiceBusClientBase() 
      {
         ConfigureForServiceBus();
      }
      public ServiceBusClientBase(string endpointName) : base(endpointName)
      {
         ConfigureForServiceBus();
      }
      public ServiceBusClientBase(Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         ConfigureForServiceBus();
      }
      public ServiceBusClientBase(string username,string password) 
      {
         ConfigureForServiceBus(username,password);
      }
      public ServiceBusClientBase(string endpointName,string username,string password) : base(endpointName)
      {
         ConfigureForServiceBus(username,password);
      }
      public ServiceBusClientBase(Binding binding,EndpointAddress remoteAddress,string username,string password) : base(binding,remoteAddress)
      {
         ConfigureForServiceBus(username,password);
      }
      protected virtual void ConfigureForServiceBus()
      {
         ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
         ServiceBusHelper.ConfigureBinding(Endpoint.Binding);
      } 
      protected virtual void ConfigureForServiceBus(string username,string password)
      {
         ClientCredentials.UserName.UserName = username;
         ClientCredentials.UserName.Password = password;
         ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
         ServiceBusHelper.ConfigureBinding(Endpoint.Binding,false);
      }
      protected override T CreateChannel()
      {
         if(Endpoint.Address.Identity == null)
         {
            string namespaceBaseAddress = ServiceBusHelper.ExtractNamespace(Endpoint.Address.Uri);
            Uri address = Endpoint.Address.Uri;
            EndpointIdentity identity = new DnsEndpointIdentity(namespaceBaseAddress);
            Endpoint.Address = new EndpointAddress(address,identity);
         }
         return base.CreateChannel();
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