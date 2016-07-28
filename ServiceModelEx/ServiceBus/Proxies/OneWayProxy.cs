// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public abstract class OneWayClientBase<T> : ServiceBusClientBase<T> where T : class
   {
      //Load service certificate from config 
      public OneWayClientBase(string username,string password) : base(username,password)
      {}
      public OneWayClientBase(string endpointName,string username,string password) : base(endpointName,username,password)
      {}
      public OneWayClientBase(NetOnewayRelayBinding binding,EndpointAddress remoteAddress,string username,string password) : base(binding,remoteAddress,username,password)
      {} 

      //Load service certificate from config and anonymous
      public OneWayClientBase() 
      {}
      public OneWayClientBase(string endpointName) : base(endpointName)
      {}
      public OneWayClientBase(NetOnewayRelayBinding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {}
      protected TransportClientEndpointBehavior ServiceBusCredentialBehavior
      {
         get
         {
            return Endpoint.Behaviors.Find<TransportClientEndpointBehavior>();
         }
      }
      protected override T CreateChannel()
      {
         ClientCredentials behavior = Endpoint.Behaviors.Find<ClientCredentials>();
         if(behavior.ServiceCertificate.DefaultCertificate == null && behavior.ServiceCertificate.ScopedCertificates.Count == 0)
         {
            SetServiceCertificate();
         }
         return base.CreateChannel();
      }
      protected void SetServiceCertificate()
      {
         SetServiceCertificate("",StoreLocation.LocalMachine,StoreName.My);
      }

      public void SetServiceCertificate(string serviceCert)
      {
         SetServiceCertificate(serviceCert,StoreLocation.LocalMachine,StoreName.My);
      }
      public void SetServiceCertificate(string serviceCert,StoreLocation location,StoreName storeName)
      {
         if(serviceCert == String.Empty)
         {
            serviceCert = ServiceBusHelper.ExtractNamespace(Endpoint.Address.Uri);
         }
         SetServiceCertificate(serviceCert,location,storeName,X509FindType.FindBySubjectName);
      }
      public void SetServiceCertificate(object findValue,StoreLocation location,StoreName storeName,X509FindType findType)
      {
         ClientCredentials behavior = Endpoint.Behaviors.Find<ClientCredentials>();
         behavior.ServiceCertificate.SetDefaultCertificate(location,storeName,findType,findValue);
         if(Endpoint.Address.Identity == null)
         {
            Uri address = Endpoint.Address.Uri;
            EndpointIdentity identity = new DnsEndpointIdentity(findValue.ToString());
            Endpoint.Address = new EndpointAddress(address,identity);
         }
      }
      
      protected override void ConfigureForServiceBus()
      {
         Debug.Assert(Endpoint.Binding is NetOnewayRelayBinding,"Must use the NetOnewayRelayBinding");
         base.ConfigureForServiceBus();
      }
      protected override void ConfigureForServiceBus(string username,string password)
      {
         Debug.Assert(Endpoint.Binding is NetOnewayRelayBinding,"Must use the NetOnewayRelayBinding");
         base.ConfigureForServiceBus(username,password);
      }
   }
}