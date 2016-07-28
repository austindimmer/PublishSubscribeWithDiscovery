// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel.Description;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Actors
{
   public class VolatileActorStateProviderAttribute : ActorStateProviderAttribute,IServiceBehavior
   {
      public void AddBindingParameters(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase,System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
      {}
      public void ApplyDispatchBehavior(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {}
      public void Validate(ServiceDescription serviceDescription,System.ServiceModel.ServiceHostBase serviceHostBase)
      {
         PersistenceProviderBehavior persistenceBehavior = serviceDescription.Behaviors.Find<PersistenceProviderBehavior>();
         if(persistenceBehavior != null)
         {
            serviceDescription.Behaviors.Remove<PersistenceProviderBehavior>();
         }
         serviceDescription.Behaviors.Add(new PersistenceProviderBehavior(new ActorInstanceProviderFactory()));
      }
   }
}

#pragma warning restore 618
