// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Configuration;
using System.Diagnostics;
using System.Messaging;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace ServiceModelEx
{
   public static class QueuedServiceHelper
   {
      public static void VerifyQueues()
      {
         Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ServiceModelSectionGroup sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);

         foreach(ChannelEndpointElement endpointElement in sectionGroup.Client.Endpoints)
         {
            if(endpointElement.Binding == "netMsmqBinding")
            {
               string queue = GetQueueFromUri(endpointElement.Address);

               if(MessageQueue.Exists(queue) == false)
               {
                  MessageQueue.Create(queue,true);
               }
            }
         }
      }

      public static void VerifyQueue<T>(string endpointName) where T : class
      {
         ChannelFactory<T> factory = new ChannelFactory<T>(endpointName);
         factory.Endpoint.VerifyQueue();
      }
      public static void VerifyQueue<T>() where T : class
      {
         VerifyQueue<T>("");
      }
      public static void VerifyQueue(this ServiceEndpoint endpoint)
      {
         if(endpoint.Binding is NetMsmqBinding)
         {
            string queue = GetQueueFromUri(endpoint.Address.Uri);

            if(MessageQueue.Exists(queue) == false)
            {
               MessageQueue.Create(queue,true);
            }
            NetMsmqBinding binding = endpoint.Binding as NetMsmqBinding;
            if(binding.DeadLetterQueue == DeadLetterQueue.Custom)
            {
               Debug.Assert(binding.CustomDeadLetterQueue != null);
               string DLQ = GetQueueFromUri(binding.CustomDeadLetterQueue);
               if(MessageQueue.Exists(DLQ) == false)
               {
                  MessageQueue.Create(DLQ,true);
               }
            }
         }
      }
      public static void PurgeQueue(ServiceEndpoint endpoint)
      {
         if(endpoint.Binding is NetMsmqBinding)
         {
            string queueName = GetQueueFromUri(endpoint.Address.Uri);

            if(MessageQueue.Exists(queueName) == true)
            {
               MessageQueue queue = new MessageQueue(queueName);
               queue.Purge();
            }
         }
      }
      internal static string GetQueueFromUri(Uri uri)
      {
         string queue = String.Empty;

         Debug.Assert(uri.Segments.Length == 3 || uri.Segments.Length == 2);
         if(uri.Segments[1] == @"private/")
         {
            queue = @".\private$\" + uri.Segments[2];
         }
         else
         {
            queue = uri.Host;
            foreach(string segment in uri.Segments)
            {
               if(segment == "/")
               {
                  continue;
               }
               string localSegment = segment;
               if(segment[segment.Length - 1] == '/')
               {
                  localSegment = segment.Remove(segment.Length - 1);
               }
               queue += @"\";
               queue += localSegment;
            }
         }
         return queue;
      }
   }
}





