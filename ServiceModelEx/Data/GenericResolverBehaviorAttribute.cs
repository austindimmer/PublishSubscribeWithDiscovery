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
   public class GenericResolverBehaviorAttribute : Attribute,IServiceBehavior
   {
      void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription,ServiceHostBase serviceHostBase,Collection<ServiceEndpoint> endpoints,BindingParameterCollection bindingParameters)
      {}

      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription,ServiceHostBase serviceHostBase)
      {}

      void IServiceBehavior.Validate(ServiceDescription serviceDescription,ServiceHostBase serviceHostBase)
      {
         ServiceHost host = serviceHostBase as ServiceHost;
         host.AddGenericResolver();
      }
   }
}