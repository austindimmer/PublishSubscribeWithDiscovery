// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class ErrorHandlerBehaviorAttribute : Attribute,IErrorHandler,IServiceBehavior
   {
      protected Type ServiceType
      {
         get;set;
      }
      
      void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase host) 
      {}
      void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase host,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
      {}
      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase host)
      {
         ServiceType = description.ServiceType;
         foreach(ChannelDispatcher dispatcher in host.ChannelDispatchers)
         {
            dispatcher.ErrorHandlers.Add(this);
         }
      }
      
      bool IErrorHandler.HandleError(Exception error)
      {
         ErrorHandlerHelper.LogError(error);
         return false;
      }
      void IErrorHandler.ProvideFault(Exception error,MessageVersion version,ref Message fault)
      {
         ErrorHandlerHelper.PromoteException(ServiceType,error,version,ref fault);
      }
   }
} 





