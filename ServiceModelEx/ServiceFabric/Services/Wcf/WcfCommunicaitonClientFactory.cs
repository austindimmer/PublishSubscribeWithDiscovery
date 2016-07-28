// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

using ServiceModelEx.ServiceFabric.Services.Client;
using ServiceModelEx.ServiceFabric.Services.Communication.Client;

namespace ServiceModelEx.ServiceFabric.Services.Communication.Wcf.Client
{
   public class WcfCommunicationClientFactory<I> : ICommunicationClientFactory<WcfCommunicationClient<I>> where I : class
   {
      public ServicePartitionResolver ServiceResolver 
      {get; private set;}
      public NetTcpBinding Binding
      {get; private set;}

      internal WcfCommunicationClientFactory(ServicePartitionResolver servicePartitionResolver = null,Binding binding = null)
      { 
         if(servicePartitionResolver == null)
         {
            ServiceResolver = ServicePartitionResolver.GetDefault();
         }
         else
         {
            ServiceResolver = servicePartitionResolver;
         }

         if(binding == null)
         {
            Binding = BindingHelper.Service.Wcf.ProxyBinding();
         }
         else
         {
            Debug.Assert(((binding as NetTcpBinding) != null));
            Binding = binding as NetTcpBinding;
         }
      }

      public WcfCommunicationClient<I> GetClient(string applicationName,string serviceName)
      {
         return new WcfCommunicationClient<I>(ServiceResolver.FabricBaseAddress,applicationName,serviceName,Binding);
      }
   }
}
