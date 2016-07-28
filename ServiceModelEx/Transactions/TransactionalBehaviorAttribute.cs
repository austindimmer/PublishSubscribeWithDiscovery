// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Persistence;

#pragma warning disable 618

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class TransactionalBehaviorAttribute : Attribute,IServiceBehavior
   {
      public bool TransactionRequiredAllOperations
      {get;set;}

      public bool AutoCompleteInstance
      {get;set;}

      public TransactionalBehaviorAttribute() 
      {
         TransactionRequiredAllOperations = true;
         AutoCompleteInstance = true;
      }
      void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase host) 
      {
         DurableServiceAttribute durable = new DurableServiceAttribute();
         durable.SaveStateInOperationTransaction = true;
         description.Behaviors.Add(durable);

         PersistenceProviderFactory factory;
         if(AutoCompleteInstance)
         {
            factory = new TransactionalInstanceProviderFactory();
         }
         else
         {
            factory = new TransactionalMemoryProviderFactory();
         }

         PersistenceProviderBehavior persistenceBehavior = new PersistenceProviderBehavior(factory);
         description.Behaviors.Add(persistenceBehavior);

         if(TransactionRequiredAllOperations)
         {
            foreach(ServiceEndpoint endpoint in description.Endpoints)
            {
               foreach(OperationDescription operation in endpoint.Contract.Operations)
               {
                  operation.Behaviors.Find<OperationBehaviorAttribute>().TransactionScopeRequired = true;
               }
            }
         }
      }
      void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase host,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
      {}
      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase host)
      {}
   }
} 
#pragma warning restore 618





