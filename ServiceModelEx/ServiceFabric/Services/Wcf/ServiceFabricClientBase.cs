// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

#if ServiceModelEx_ServiceFabric
using ServiceModelEx.ServiceFabric.Services.Client;
using ServiceModelEx.ServiceFabric.Services.Communication.Client;
#else
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
#endif

namespace ServiceModelEx.ServiceFabric.Services.Communication.Wcf.Client
{
   public class ServiceFabricClientBase<I> : ServicePartitionClient<WcfCommunicationClient<I>> where I : class
   {
      public ServiceFabricClientBase(Uri address) : this(null,address)
      {}
      public ServiceFabricClientBase(ServicePartitionResolver resolver,Uri address) : base(new WcfCommunicationClientFactory<I>(resolver),address)
      {
         ListenerName = typeof(I).Name;
      }
   }
}
