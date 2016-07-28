// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Security;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public abstract class ServiceBusDuplexClientBase<T,C> : DuplexClientBase<T,C>,IServiceBusProperties where T : class
   {
      public ServiceBusDuplexClientBase(C callback) : base(callback) 
      {
         ConfigureForServiceBus();
      }
      public ServiceBusDuplexClientBase(C callback,string endpointName) : base(callback,endpointName)
      {
         ConfigureForServiceBus();
      }
      public ServiceBusDuplexClientBase(C callback,NetTcpRelayBinding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {
         ConfigureForServiceBus();
      }
      public ServiceBusDuplexClientBase(C callback,string username,string password) : base(callback)
      {
         ConfigureForServiceBus(username,password);
      }
      public ServiceBusDuplexClientBase(C callback,string endpointName,string username,string password) : base(callback,endpointName)
      {
         ConfigureForServiceBus(username,password);
      }
      public ServiceBusDuplexClientBase(C callback,NetTcpRelayBinding binding,EndpointAddress remoteAddress,string username,string password) : base(callback,binding,remoteAddress)
      {
         ConfigureForServiceBus(username,password);
      }
      protected virtual void ConfigureForServiceBus()
      {
         Debug.Assert(Endpoint.Binding is NetTcpRelayBinding);
         ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
         ServiceBusHelper.ConfigureBinding(Endpoint.Binding);
      } 
      protected virtual void ConfigureForServiceBus(string username,string password)
      {
         Debug.Assert(Endpoint.Binding is NetTcpRelayBinding);
         ClientCredentials.UserName.UserName = username;
         ClientCredentials.UserName.Password = password;
         ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
         ServiceBusHelper.ConfigureBinding(Endpoint.Binding,false);
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