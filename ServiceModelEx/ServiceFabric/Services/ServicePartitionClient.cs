// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Threading.Tasks;

namespace ServiceModelEx.ServiceFabric.Services.Communication.Client
{
   public class ServicePartitionClient<T> where T : ICommunicationClient
   {
      string ApplicationName
      {get; set;}
      string ServiceName
      {get; set;}
      protected T Client
      {get; set;}

      protected string ListenerName
      {get;set; }

      protected ServicePartitionClient(ICommunicationClientFactory<T> factory,Uri serviceAddress)
      {
         string applicationName = string.Empty, 
                serviceName = string.Empty;

         AddressHelper.EvaluateAddress(serviceAddress,out applicationName,out serviceName);

         ApplicationName = applicationName;
         ServiceName = serviceName;
         Client = factory.GetClient(applicationName,serviceName);
      }
      protected void InvokeWithRetry(Action<T> invoke)
      {
         invoke(Client);
      }
      protected R InvokeWithRetry<R>(Func<T,R> invoke)
      {
         return invoke(Client);
      }
      protected Task InvokeWithRetryAsync(Func<T,Task> invoke)
      {
         return invoke(Client);
      }
      protected Task<R> InvokeWithRetryAsync<R>(Func<T,Task<R>> invoke)
      {
         return invoke(Client);
      }
   }
}
