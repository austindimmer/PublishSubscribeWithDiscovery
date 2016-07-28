// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Reflection;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using ServiceModelEx.ServiceFabric.Actors;

namespace ServiceModelEx.ServiceFabric.Test
{
   internal class TestActorServiceBehavior<S> : IServiceBehavior,IInstanceProvider where S : class,new()
   {
      S State 
      {get;set;}

      public TestActorServiceBehavior(S state)
      {
         State = state;
      }

      object SetState(object instance)
      {
         StatefulActor<S> actor = instance as StatefulActor<S>;
         if(actor != null)
         {
            actor.GetType().InvokeMember("State",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.SetProperty,null,actor,new object[] {State});
         }  
         return instance;
      }
      public object GetInstance(System.ServiceModel.InstanceContext instanceContext,System.ServiceModel.Channels.Message message)
      {
         object instance = Activator.CreateInstance(instanceContext.Host.Description.ServiceType);
         return SetState(instance);
      }
      public object GetInstance(System.ServiceModel.InstanceContext instanceContext)
      {
         object instance = Activator.CreateInstance(instanceContext.Host.Description.ServiceType);
         return SetState(instance);
      }
      public void ReleaseInstance(System.ServiceModel.InstanceContext instanceContext,object instance)
      {}

      class InstanceProviderBehavior : IEndpointBehavior
      {
         IInstanceProvider InstanceProvider
         {get;set;}
         public InstanceProviderBehavior(IInstanceProvider instanceProivder)
         {
            InstanceProvider = instanceProivder;
         }

         public void AddBindingParameters(ServiceEndpoint endpoint,System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
         {}
         public void ApplyClientBehavior(ServiceEndpoint endpoint,ClientRuntime clientRuntime)
         {}
         public void ApplyDispatchBehavior(ServiceEndpoint endpoint,EndpointDispatcher endpointDispatcher)
         {
            endpointDispatcher.DispatchRuntime.InstanceProvider = InstanceProvider;
         }
         public void Validate(ServiceEndpoint endpoint)
         {}
      }

      public void AddBindingParameters(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase,System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
      {}
      public void ApplyDispatchBehavior(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {
         foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
         {
            endpoint.EndpointBehaviors.Add(new InstanceProviderBehavior(this));
         }
      }
      public void Validate(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {
         if(serviceDescription.Behaviors.Find<ActorStateProviderAttribute>() != null)
         {
            serviceDescription.Behaviors.Remove<ActorStateProviderAttribute>();
         }
      }
   }
}
