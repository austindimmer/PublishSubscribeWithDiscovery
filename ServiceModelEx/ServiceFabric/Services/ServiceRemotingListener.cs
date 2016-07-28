// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

using ServiceModelEx.Fabric;
using ServiceModelEx.ServiceFabric.Services.Communication.Runtime;

namespace ServiceModelEx.ServiceFabric.Services.Remoting.Runtime
{
   public class ServiceRemotingListener<I> : ICommunicationListener where I : class,IService
   {
      //Do nothing with instance. Do not support singleton services.
      public ServiceRemotingListener(ServiceInitializationParameters initParams,I instance)
      {}
   }
}
