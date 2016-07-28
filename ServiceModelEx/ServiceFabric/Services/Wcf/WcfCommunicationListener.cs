// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;

using ServiceModelEx.Fabric;
using ServiceModelEx.ServiceFabric.Services.Communication.Runtime;

namespace ServiceModelEx.ServiceFabric.Services.Communication.Wcf.Runtime
{
   public class WcfCommunicationListener : ICommunicationListener
   {
      internal Type InterfaceType
      {get;private set;}
      internal Type ImplementationType
      {get;private set;}

      public NetTcpBinding Binding 
      {get; set;}

      //Do not support singleton ctor.
      public WcfCommunicationListener(ServiceInitializationParameters initParams,Type interfaceType,Type implementationType)
      {
         InterfaceType = interfaceType;
         ImplementationType = implementationType;
         Binding = BindingHelper.Service.Wcf.ServiceBinding();
      }
   }
}
