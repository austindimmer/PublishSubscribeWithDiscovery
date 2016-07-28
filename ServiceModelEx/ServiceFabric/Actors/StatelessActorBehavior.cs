// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Actors
{
   internal class StatelessActorBehavior : IServiceBehavior
   {
      public void AddBindingParameters(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase,System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
      {}
      public void ApplyDispatchBehavior(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {}
      public void Validate(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {
         EnforceStatelessActorOperationBehaviorPolicy(serviceDescription);
         EnforceStatelessActorBehaviorPolicy(serviceDescription);
      }

      void EnforceStatelessActorBehaviorPolicy(ServiceDescription serviceDescription)
      {
         ServiceBehaviorAttribute serviceBehavior = serviceDescription.Behaviors.FirstOrDefault(behavior => behavior is ServiceBehaviorAttribute) as ServiceBehaviorAttribute;
         if(serviceBehavior != null)
         {
            serviceBehavior.InstanceContextMode = InstanceContextMode.PerCall;
            serviceBehavior.ConcurrencyMode = ConcurrencyMode.Single;
            serviceBehavior.MaxItemsInObjectGraph = int.MaxValue;
            serviceBehavior.UseSynchronizationContext = false;
         }
         serviceDescription.SetThrottle();
      }
      void EnforceStatelessActorOperationBehaviorPolicy(ServiceDescription serviceDescription)
      {
         if(serviceDescription.Endpoints.Count(endpoint=>!endpoint.Contract.ContractType.Namespace.Contains("ServiceModelEx")) > 1)
         {
            throw new InvalidOperationException("Validation failed. Multiple application interfaces found. An actor may only possess a single application interface.");
         }

         foreach(ServiceEndpoint endpoint in serviceDescription.Endpoints)
         {
            endpoint.EndpointBehaviors.Add(new FabricThreadPoolBehavior());

            foreach(OperationDescription operation in endpoint.Contract.Operations)
            {
               if(operation.TaskMethod == null)
               {
                  throw new InvalidOperationException("Validation failed. Actor operation '" + endpoint.Contract.ContractType.FullName + "." + operation.Name + "' does not return Task or Task<>. Actor interface methods must be async and must return either Task or Task<>." );
               }
               if((!typeof(IActor).GetMembers().Any(member=>member.Name.Contains(operation.Name))) && (!operation.IsInitiating))
               {
                  throw new InvalidOperationException("Validation failed. Stateless Actor operations cannot be non-initiating. Operation: " + endpoint.Contract.ContractType.FullName + "." + operation.Name);
               }
               if(operation.TaskMethod.GetCustomAttributes<CompletesActorInstanceAttribute>().Any())
               {
                  throw new InvalidOperationException("Validation failed. Cannot apply CompletesActorInstanceAttribute to stateless actors.");
               }

               OperationBehaviorAttribute operationBehavior = operation.OperationBehaviors.FirstOrDefault(behavior=>behavior is OperationBehaviorAttribute) as OperationBehaviorAttribute;

               Debug.Assert(operationBehavior.TransactionScopeRequired == false);
               operationBehavior.TransactionScopeRequired = false;

               operation.OperationBehaviors.Add(new ActorOperationBehavior());
            }
         }
      }
   }
}

#pragma warning restore 618