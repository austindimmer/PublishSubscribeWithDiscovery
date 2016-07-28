// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;

namespace ServiceModelEx
{
   public class InstanceContext<T> 
   {
      public InstanceContext Context
      {get;private set;}

      public InstanceContext(T callbackInstance)
      {
         Context = new InstanceContext(callbackInstance);
      }
      public void ReleaseServiceInstance()
      {
         Context.ReleaseServiceInstance();
      }
      public T ServiceInstance
      {
         get
         {
            return (T)Context.GetServiceInstance();
         }
      }
   }
}
