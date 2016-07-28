// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceModelEx
{
   public class PersistentSubscriptionServiceClient : ClientBase<IPersistentSubscriptionService>,IPersistentSubscriptionService
   {
      public PersistentSubscriptionServiceClient()
      {}

      public PersistentSubscriptionServiceClient(string endpointConfigurationName) : base(endpointConfigurationName)
      {}

      public PersistentSubscriptionServiceClient(string endpointConfigurationName,string remoteAddress) : base(endpointConfigurationName,remoteAddress)
      {}

      public PersistentSubscriptionServiceClient(string endpointConfigurationName,EndpointAddress remoteAddress) : base(endpointConfigurationName,remoteAddress)
      {}

      public PersistentSubscriptionServiceClient(Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {}

      public void Subscribe(string address,string eventsContract,string eventOperation)
      {
         Channel.Subscribe(address,eventsContract,eventOperation);
      }

      public void Unsubscribe(string address,string eventsContract,string eventOperation)
      {
         Channel.Unsubscribe(address,eventsContract,eventOperation);
      }

      public PersistentSubscription[] GetAllSubscribers()
      {
         return Channel.GetAllSubscribers();
      }

      public PersistentSubscription[] GetSubscribersToContract(string eventsContract)
      {
         return Channel.GetSubscribersToContract(eventsContract);
      }

      public string[] GetSubscribersToContractEventType(string eventsContract,string eventOperation)
      {
         return Channel.GetSubscribersToContractEventType(eventsContract,eventOperation);
      }

      public PersistentSubscription[] GetAllSubscribersFromAddress(string address)
      {
         return Channel.GetAllSubscribersFromAddress(address);
      }
   }
}
