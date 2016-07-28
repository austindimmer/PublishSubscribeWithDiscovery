// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Collections.Specialized;
using System.ServiceModel.Description;
using System.ServiceModel.Persistence;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Actors
{
   public class ReliableDictionaryActorStateProviderAttribute : ActorStateProviderAttribute,IServiceBehavior
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
         serviceDescription.Behaviors.Add(new PersistenceProviderBehavior(new SqlPersistenceProviderFactory(new NameValueCollection
                                                                                                            {
                                                                                                               {"connectionStringName","DurableServices"},
                                                                                                               {"serializeAsText","true"}
                                                                                                            })));
      }
   }
}

#pragma warning restore 618
