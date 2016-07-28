// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Actors
{
   internal class StatefulActorBehavior : IServiceBehavior
   {
      public void AddBindingParameters(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase,System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
      {}
      public void ApplyDispatchBehavior(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {}
      public void Validate(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {
         EnforceStatefulActorEndpointBehaviorPolicy(serviceDescription);
         EnforceStatefulActorBehaviorPolicy(serviceDescription);
      }

      void EnforceStatefulActorBehaviorPolicy(ServiceDescription serviceDescription)
      {
         ServiceBehaviorAttribute serviceBehavior = serviceDescription.Behaviors.Find<ServiceBehaviorAttribute>();
         if(serviceBehavior != null)
         {
            Debug.Assert(serviceBehavior.InstanceContextMode == InstanceContextMode.PerSession);
            Debug.Assert(serviceBehavior.ConcurrencyMode == ConcurrencyMode.Single);

            serviceBehavior.InstanceContextMode = InstanceContextMode.PerSession;
            serviceBehavior.ConcurrencyMode = ConcurrencyMode.Single;
            serviceBehavior.MaxItemsInObjectGraph = int.MaxValue;
            serviceBehavior.UseSynchronizationContext = false;
         }
         DurableServiceAttribute durableService = new DurableServiceAttribute();
         durableService.SaveStateInOperationTransaction = true;
         serviceDescription.Behaviors.Add(durableService);
         if(!serviceDescription.Behaviors.Any(behavior=>behavior is ActorStateProviderAttribute))
         {
            serviceDescription.Behaviors.Add(new VolatileActorStateProviderAttribute());
         }
         serviceDescription.SetThrottle();
      }
      void EnforceStatefulActorEndpointBehaviorPolicy(ServiceDescription serviceDescription)
      {
         if(serviceDescription.Endpoints.Count(endpoint=>!endpoint.Contract.ContractType.Namespace.Contains("ServiceModelEx")) > 1)
         {
            throw new InvalidOperationException("Validation failed. Multiple application interfaces found. An Actor may only possess a single application interface.");
         }

         ActorInstanceContextProvider contextProvider = new ActorInstanceContextProvider();
         foreach(ServiceEndpoint endpoint in serviceDescription.Endpoints)
         {
            //All endpoints for a given Actor must share the same ActorInstanceContextProvider.
            endpoint.EndpointBehaviors.Add(contextProvider);
            endpoint.EndpointBehaviors.Add(new FabricThreadPoolBehavior());

            foreach(OperationDescription operation in endpoint.Contract.Operations)
            {
               if(operation.TaskMethod == null)
               {
                  throw new InvalidOperationException("Validation failed. Actor operation '" + endpoint.Contract.ContractType.FullName + "." + operation.Name + "' does not return Task or Task<>. Actor interface methods must be async and must return either Task or Task<>." );
               }

               OperationBehaviorAttribute operationBehavior = operation.OperationBehaviors.FirstOrDefault(behavior=>behavior is OperationBehaviorAttribute) as OperationBehaviorAttribute;
               operationBehavior.TransactionScopeRequired = true;
               operation.OperationBehaviors.Add(new ActorOperationBehavior());
            }
         }
      }
   }
}

#pragma warning restore 618