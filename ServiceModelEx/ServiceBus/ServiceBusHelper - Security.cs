// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public static partial class ServiceBusHelper
   {
      public const string DefaultIssuer = "owner";

      static void SetServiceBusCredentials(IEnumerable<ServiceEndpoint> endpoints,string issuer,string secret)
      {
         TransportClientEndpointBehavior behavior = new TransportClientEndpointBehavior();
         TokenProvider tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);
         SetServiceBusCredentials(endpoints,tokenProvider);
      }

      static void SetServiceBusCredentials(IEnumerable<ServiceEndpoint> endpoints,TokenProvider tokenProvider)
      {
         TransportClientEndpointBehavior behavior = new TransportClientEndpointBehavior();
         behavior.TokenProvider = tokenProvider;

         SetBehavior(endpoints,behavior);
      }
      
      public static void SetServiceBusCredentials<T>(this ClientBase<T> proxy,string secret) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy is already opened");
         }
         proxy.SetServiceBusCredentials(DefaultIssuer,secret);
      }
      public static void SetServiceBusCredentials<T>(this ClientBase<T> proxy,string issuer,string secret) where T : class
      {
         TokenProvider tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);
         SetServiceBusCredentials(proxy,tokenProvider);
      }


      public static void SetServiceBusCredentials<T>(this ClientBase<T> proxy,TokenProvider tokenProvider) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy is already opened");
         }
         proxy.ChannelFactory.SetServiceBusCredentials(tokenProvider);
      }

      public static void SetServiceBusCredentials<T>(this ChannelFactory<T> factory,string issuer,string secret) where T : class
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Factory is already opened");
         }

         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SetServiceBusCredentials(endpoints,issuer,secret);
      }
      public static void SetServiceBusCredentials<T>(this ChannelFactory<T> factory,string secret) where T : class
      {
         factory.SetServiceBusCredentials(DefaultIssuer,secret);
      }
      static void SetServiceBusCredentials<T>(this ChannelFactory<T> factory,TokenProvider tokenPorvider) where T : class
      {
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SetServiceBusCredentials(endpoints,tokenPorvider);
      }
      public static void SetServiceBusCredentials(this ServiceHost host,string secret)
      {
         if(host.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }
         TokenProvider tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(DefaultIssuer,secret);
         SetServiceBusCredentials(host.Description.Endpoints,tokenProvider);
      }

      public static void SetServiceBusCredentials(this ServiceHost host,TokenProvider tokenProvider)
      {
         if(host.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }
         SetServiceBusCredentials(host.Description.Endpoints,tokenProvider);
      }

      public static void SetServiceBusCredentials(this ServiceHost host,string issuer,string secret)
      {
         TokenProvider tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);

         SetServiceBusCredentials(host.Description.Endpoints,issuer,secret);
      }      

      public static void SetServiceBusCredentials(this MetadataExchangeClient mexClient,string secret)
      {
         SetServiceBusCredentials(mexClient,DefaultIssuer,secret);
      }
      public static void SetServiceBusCredentials(this MetadataExchangeClient mexClient,string issuer,string secret)
      {
         TokenProvider tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);
         SetServiceBusCredentials(mexClient,tokenProvider);
      }
      public static void SetServiceBusCredentials(this MetadataExchangeClient mexClient,TokenProvider tokenProvider)
      {
         Type type = mexClient.GetType();
         FieldInfo info = type.GetField("factory",BindingFlags.Instance|BindingFlags.NonPublic);
         ChannelFactory<IMetadataExchange> factory = info.GetValue(mexClient) as ChannelFactory<IMetadataExchange>;
         factory.SetServiceBusCredentials(tokenProvider);
      }       
   }
}





