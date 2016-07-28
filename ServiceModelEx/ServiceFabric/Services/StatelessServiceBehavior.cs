// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Services
{
   internal class StatelessServiceBehavior : IServiceBehavior
   {
      public void AddBindingParameters(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase,System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
      {}
      public void ApplyDispatchBehavior(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {}
      public void Validate(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {
         EnforceStatelessServiceOperationBehaviorPolicy(serviceDescription);
         EnforceStatelessServiceBehaviorPolicy(serviceDescription);
      }

      void EnforceStatelessServiceBehaviorPolicy(ServiceDescription serviceDescription)
      {
         ServiceBehaviorAttribute serviceBehavior = serviceDescription.Behaviors.FirstOrDefault(behavior => behavior is ServiceBehaviorAttribute) as ServiceBehaviorAttribute;
         if(serviceBehavior != null)
         {
            serviceBehavior.InstanceContextMode = InstanceContextMode.PerCall;
            serviceBehavior.ConcurrencyMode = ConcurrencyMode.Multiple;
            serviceBehavior.MaxItemsInObjectGraph = int.MaxValue;
            serviceBehavior.UseSynchronizationContext = false;
         }
         serviceDescription.SetThrottle();
      }
      void EnforceStatelessServiceOperationBehaviorPolicy(ServiceDescription serviceDescription)
      {
         foreach(ServiceEndpoint endpoint in serviceDescription.Endpoints)
         {
            endpoint.EndpointBehaviors.Add(new FabricThreadPoolBehavior());

            foreach(OperationDescription operation in endpoint.Contract.Operations)
            {
               if(operation.TaskMethod == null)
               {
                  throw new InvalidOperationException("Validation failed. Service operation '" + endpoint.Contract.ContractType.FullName + "." + operation.Name + "' does not return Task or Task<>. Service interface methods must be async and must return either Task or Task<>.");
               }
               OperationBehaviorAttribute operationBehavior = operation.OperationBehaviors.FirstOrDefault(behavior=>behavior is OperationBehaviorAttribute) as OperationBehaviorAttribute;

               Debug.Assert(operationBehavior.TransactionScopeRequired == false);
               operationBehavior.TransactionScopeRequired = false;

               operation.OperationBehaviors.Add(new ServiceOperationBehavior());
            }
         }
      }
   }
}

#pragma warning restore 618