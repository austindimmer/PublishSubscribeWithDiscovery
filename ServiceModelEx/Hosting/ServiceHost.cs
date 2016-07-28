// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   public class ServiceHost<T> : ServiceHost
   {
      class ErrorHandlerBehavior : IServiceBehavior,IErrorHandler
      {
         IErrorHandler m_ErrorHandler;
         public ErrorHandlerBehavior(IErrorHandler errorHandler)
         {
            m_ErrorHandler = errorHandler;
         }
         void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase host)
         {}
         void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase host,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
         {}
         void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase host)
         {
            foreach(ChannelDispatcher dispatcher in host.ChannelDispatchers)
            {
               dispatcher.ErrorHandlers.Add(this);
            }
         }
         bool IErrorHandler.HandleError(Exception error)
         {
            return m_ErrorHandler.HandleError(error);
         }
         void IErrorHandler.ProvideFault(Exception error,MessageVersion version,ref System.ServiceModel.Channels.Message fault)
         {
            m_ErrorHandler.ProvideFault(error,version,ref fault);
         }
      }

      List<IServiceBehavior> m_ErrorHandlers = new List<IServiceBehavior>();

      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public void AddErrorHandler(IErrorHandler errorHandler)
      {
         if(State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }
         Debug.Assert(errorHandler != null);
         IServiceBehavior errorHandlerBehavior = null;
         if(errorHandler is IServiceBehavior)
         {
            errorHandlerBehavior = errorHandler as IServiceBehavior;
         }
         else
         {
            errorHandlerBehavior = new ErrorHandlerBehavior(errorHandler);
         }
         m_ErrorHandlers.Add(errorHandlerBehavior);
      }
      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public void AddErrorHandler()
      {
         AddErrorHandler(new ErrorHandlerBehaviorAttribute());
      } 

      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public void EnableMetadataExchange(bool enableHttpGet = true)
      {
         if(State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }

         ServiceMetadataBehavior metadataBehavior;
         metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();

         if(metadataBehavior == null)
         {
            metadataBehavior = new ServiceMetadataBehavior();
            Description.Behaviors.Add(metadataBehavior);
                                                            
            if(BaseAddresses.Any((uri)=>uri.Scheme == "http"))
            {
               metadataBehavior.HttpGetEnabled = enableHttpGet;
            }
                                              
            if(BaseAddresses.Any((uri)=>uri.Scheme == "https"))
            {
               metadataBehavior.HttpsGetEnabled = enableHttpGet;
            }
         }
         AddAllMexEndPoints();
      }
      public void AddAllMexEndPoints()
      {
         Debug.Assert(HasMexEndpoint == false);

         foreach(Uri baseAddress in BaseAddresses)
         {
            Binding binding = null;
            switch(baseAddress.Scheme)
            {
               case "net.tcp":
               {
                  binding = MetadataExchangeBindings.CreateMexTcpBinding();
                  break;
               }
               case "net.pipe":
               {
                  binding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
                  break;
               }
               case "http":
               {
                  binding = MetadataExchangeBindings.CreateMexHttpBinding();
                  break;
               }
               case "https":
               {
                  binding = MetadataExchangeBindings.CreateMexHttpsBinding();
                  break;
               }
            }
            if(binding != null)
            {
               AddServiceEndpoint(typeof(IMetadataExchange),binding,"MEX");
            }         
         }
      }
      
      public bool HasMexEndpoint
      {
         get
         {
            return Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IMetadataExchange));
         }
      }
      
      protected override void OnOpening()
      {
         this.AddGenericResolver();

         foreach(IServiceBehavior behavior in m_ErrorHandlers)
         {
            Description.Behaviors.Add(behavior);
         }

         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {
            endpoint.VerifyQueue(); 
         } 
         base.OnOpening();
      }
      protected override void OnClosing()
      {
         PurgeQueues();
         base.OnClosing();
      }
      [Conditional("DEBUG")]
      void PurgeQueues()
      {
         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {
            QueuedServiceHelper.PurgeQueue(endpoint);
         }
      }
        
      /// <summary>
      /// Can only call after openning the host
      /// </summary>
      public ServiceThrottle Throttle
      {
         get
         {
            if(State != CommunicationState.Opened)
            {
               throw new InvalidOperationException("Host is not opened");
            }

            ChannelDispatcher dispatcher = OperationContext.Current.Host.ChannelDispatchers[0] as ChannelDispatcher;
            return dispatcher.ServiceThrottle;
         }
      } 
      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public bool IncludeExceptionDetailInFaults
      {
         set
         {
            if(State == CommunicationState.Opened)
            {
               throw new InvalidOperationException("Host is already opened");
            }
            ServiceBehaviorAttribute debuggingBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
            debuggingBehavior.IncludeExceptionDetailInFaults = value;
         }
         get
         {
            ServiceBehaviorAttribute debuggingBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
            return debuggingBehavior.IncludeExceptionDetailInFaults;
         }
      }

       /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public bool SecurityAuditEnabled
      {
         get
         {
            ServiceSecurityAuditBehavior securityAudit = Description.Behaviors.Find<ServiceSecurityAuditBehavior>();
            if(securityAudit != null)
            {
               return securityAudit.MessageAuthenticationAuditLevel == AuditLevel.SuccessOrFailure &&
                      securityAudit.ServiceAuthorizationAuditLevel == AuditLevel.SuccessOrFailure;
            }
            else
            {
               return false;
            }
         }
         set
         {
            if(State == CommunicationState.Opened)
            {
               throw new InvalidOperationException("Host is already opened");
            }
            ServiceSecurityAuditBehavior securityAudit = Description.Behaviors.Find<ServiceSecurityAuditBehavior>();
            if(securityAudit == null && value == true)
            {
               securityAudit = new ServiceSecurityAuditBehavior();
               securityAudit.MessageAuthenticationAuditLevel = AuditLevel.SuccessOrFailure;
               securityAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;
               Description.Behaviors.Add(securityAudit);
            }
         }
      }
      public ServiceHost() : base(typeof(T))
      {}
      public ServiceHost(params string[] baseAddresses) : base(typeof(T),baseAddresses.Select((address)=>new Uri(address)).ToArray())
      {}
      public ServiceHost(params Uri[] baseAddresses) : base(typeof(T),baseAddresses)
      {}
      public ServiceHost(T singleton,params string[] baseAddresses) : base(singleton,baseAddresses.Select((address)=>new Uri(address)).ToArray())
      {}
      public ServiceHost(T singleton) : base(singleton)
      {}
      public ServiceHost(T singleton,params Uri[] baseAddresses) : base(singleton,baseAddresses)
      {}
      public virtual T Singleton
      {
         get
         {
            if(SingletonInstance == null)
            {
               return default(T);
            }
            Debug.Assert(SingletonInstance is T);
            return (T)SingletonInstance;
         }
      }
   }
}





