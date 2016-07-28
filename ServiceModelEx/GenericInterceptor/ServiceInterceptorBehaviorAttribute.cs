// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public abstract class ServiceInterceptorBehaviorAttribute : Attribute,IServiceBehavior
   {
      protected abstract OperationInterceptorBehaviorAttribute CreateOperationInterceptor();

      public void ApplyDispatchBehavior(ServiceDescription serviceDescription,ServiceHostBase host)
      {
         foreach(ServiceEndpoint endpoint in serviceDescription.Endpoints)
         {
            foreach(OperationDescription operation in endpoint.Contract.Operations)
            {
               if(operation.Behaviors.Find<OperationInterceptorBehaviorAttribute>() != null)
               {
                  continue;
               }
               operation.Behaviors.Add(CreateOperationInterceptor());
            }
         }
      }
      public void AddBindingParameters(ServiceDescription serviceDescription,ServiceHostBase serviceHostBase,Collection<ServiceEndpoint> endpoints,BindingParameterCollection bindingParameters)
      {}

      public void Validate(ServiceDescription serviceDescription,ServiceHostBase serviceHostBase)
      {}
   }
}