// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.ServiceModel;

#if ServiceModelEx_ServiceFabric
using ServiceModelEx.Fabric;
using ServiceModelEx.ServiceFabric.Services.Communication.Runtime;
#else
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
#endif

namespace ServiceModelEx.ServiceFabric.Services.Communication.Wcf.Runtime
{
   //TODO: Not just for WCF. Switch listener on ServiceContractAttribute.
   public static class WcfHelper
   {
      public static IEnumerable<ServiceInstanceListener> CreateListeners<T>() where T : class
      {
         List<ServiceInstanceListener> listeners = new List<ServiceInstanceListener>();

         foreach(Type contractType in typeof(T).GetInterfaces().Where(contract=>contract.GetCustomAttributes(typeof(ServiceContractAttribute),false).Length > 0))
         {
            Func<StatelessServiceInitializationParameters,ICommunicationListener> createListener = null;
            createListener = (initParams)=>new WcfCommunicationListener(initParams,contractType,typeof(T));
            listeners.Add(new ServiceInstanceListener(createListener,contractType.Name));
         }
         return listeners;
      }
   }
}
