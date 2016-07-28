// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Description;
using Microsoft.ServiceBus.Messaging;


namespace ServiceModelEx.ServiceBus
{
   static partial class ServiceBusHelper
   {
      public static void CreateQueue(Uri queueAddress,string secret,bool requiresSession = false)
      {
         Uri baseAddress = ParseUri(queueAddress).Item1;
         string queueName = ParseUri(queueAddress).Item2;

         CreateQueue(baseAddress,queueName,ServiceBusHelper.DefaultIssuer,secret,requiresSession);
      }
      public static void CreateQueue(Uri baseAddress,string queueName,string secret,bool requiresSession = false)
      {
         CreateQueue(baseAddress,queueName,ServiceBusHelper.DefaultIssuer,secret,requiresSession);
      }
      public static void CreateQueue(Uri baseAddress,string queueName,string issuer,string secret,bool requiresSession = false)
      {
         TokenProvider credentials = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);
         CreateQueue(baseAddress,queueName,credentials,requiresSession);
      }

      static void CreateQueue(Uri baseAddress,string queueName,TokenProvider credentials,bool requiresSession = false)
      {
         string address = baseAddress.AbsoluteUri;
         if(address.EndsWith("/") == false)
         {
            address += "/";
         }
         address += queueName;

         QueueDescription queueDescription = CreateQueueDescription(queueName,requiresSession);

         Tuple<Uri,string> tuple = ParseUri(new Uri(address));
         CreateQueue(baseAddress,queueName,queueDescription,credentials);
      }

      static void CreateQueue(Uri baseAddress,string queueName,QueueDescription queueDescription,TokenProvider credentials)
      { 
         if(QueueExists(baseAddress,queueName,credentials))
         {
            DeleteQueue(baseAddress,queueName,credentials);
         }  
         NamespaceManager namespaceClient = new NamespaceManager(baseAddress,credentials);
         namespaceClient.CreateQueue(queueDescription);
      }

      public static void DeleteQueue(Uri queueAddress,string secret)
      {
         Uri baseAddress = ParseUri(queueAddress).Item1;
         string queueName = ParseUri(queueAddress).Item2;

         DeleteQueue(baseAddress,queueName,ServiceBusHelper.DefaultIssuer,secret);
      }
      public static void DeleteQueue(Uri baseAddress,string queueName,string secret)
      {
         DeleteQueue(baseAddress,queueName,ServiceBusHelper.DefaultIssuer,secret);
      }
      public static void DeleteQueue(Uri baseAddress,string queueName,string issuer,string secret)
      {
         TokenProvider credentials = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);
         DeleteQueue(baseAddress,queueName,credentials);
      }  
      
      public static void VerifyQueue(Uri queueAddress,string secret,bool requiresSession = false)
      {
         VerifyQueue(queueAddress,ServiceBusHelper.DefaultIssuer,secret,requiresSession);
      }
      public static void VerifyQueue(Uri queueAddress,string issuer,string secret,bool requiresSession = false)
      {
         Uri baseAddress = ParseUri(queueAddress).Item1;
         string queueName = ParseUri(queueAddress).Item2;

         VerifyQueue(baseAddress,queueName,issuer,secret,requiresSession);
      }
      public static void VerifyQueue(Uri baseAddress,string queueName,string issuer,string secret,bool requiresSession = false)
      {
         TokenProvider credentials = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);

         VerifyQueue(baseAddress,queueName,credentials,requiresSession);
      }
 
      public static void PurgeQueue(Uri baseAddress,string queueName,string secret)
      {
         PurgeQueue(baseAddress,queueName,ServiceBusHelper.DefaultIssuer,secret);
      }
      public static void PurgeQueue(Uri baseAddress,string queueName,string issuer,string secret)
      {
         TokenProvider credentials = TokenProvider.CreateSharedSecretTokenProvider(issuer,secret);

         PurgeQueue(baseAddress,queueName,credentials);
      }
      public static void PurgeQueue(Uri baseAddress,string queueName,TokenProvider credentials)
      {
         Debug.Assert(QueueExists(baseAddress,queueName,credentials));

         NamespaceManager namespaceClient = new NamespaceManager(baseAddress,credentials);
         QueueDescription description = namespaceClient.GetQueue(queueName);

         DeleteQueue(baseAddress,queueName,credentials);

         CreateQueue(baseAddress,queueName,description,credentials);
      }
      //Helpers

      static void DeleteQueue(Uri baseAddress,string queueName,TokenProvider credentials)
      {
         if(baseAddress.AbsoluteUri.EndsWith("/") == false)
         {
            baseAddress = new Uri(baseAddress.AbsoluteUri + "/");
         }         
         Debug.Assert(baseAddress.Scheme == "sb");

         if(QueueExists(baseAddress,queueName,credentials))
         {
            NamespaceManager namespaceClient = new NamespaceManager(baseAddress,credentials);
            namespaceClient.DeleteQueue(queueName);
         }  
      }
      static internal QueueDescription CreateQueueDescription(string queueName,bool requiresSession = false)
      {
         QueueDescription description = new QueueDescription(queueName);
         description.RequiresSession = requiresSession;
         description.DefaultMessageTimeToLive = TimeSpan.FromDays(1);

         return description;
      }
      internal static void VerifyQueue(Uri baseAddress,string queueName,TokenProvider credential,bool requiresSession = false)
      {
         if(QueueExists(baseAddress,queueName,credential,requiresSession))
         {
            return;
         }
         string address = baseAddress.AbsoluteUri;
         if(address.EndsWith("/") == false)
         {
            address += "/";
         }
         address += queueName;

         CreateQueue(baseAddress,queueName,credential,requiresSession);
      }
      static bool QueueExists(Uri baseAddress,string queueName,TokenProvider credentials,bool requiresSession = false)
      {         
         try
         {
            NamespaceManager namespaceClient = new NamespaceManager(baseAddress,credentials);
            QueueDescription queue = namespaceClient.GetQueue(queueName);
            return true;
         }
         catch(MessagingEntityNotFoundException)
         {}
         return false;
      }


      internal static Tuple<Uri,string> ParseUri(Uri address)
      {
         int offset = 0;

         if(address.AbsoluteUri.EndsWith("/"))
         {
            offset = 1;
         }

         string[] segments = address.AbsoluteUri.Split('/');
         string baseAddress = String.Empty;

         string queueName = segments[segments.Length-1-offset];
         baseAddress = address.AbsoluteUri.Substring(0,address.AbsoluteUri.Length-queueName.Length-offset);
         
         return new Tuple<Uri,string>(new Uri(baseAddress),queueName);
      }
   }
}






