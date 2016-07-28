// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Collections.Generic;
using ServiceModelEx.ServiceFabric.Services.Communication.Runtime;

namespace ServiceModelEx.ServiceFabric.Services.Runtime
{
   public abstract class StatelessService
   {
      public StatelessService()
      {
         ServiceInstanceListeners = CreateServiceInstanceListeners();
      }
      protected abstract IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners();
      public IEnumerable<ServiceInstanceListener> ServiceInstanceListeners 
      {get; private set;}
   }
}
