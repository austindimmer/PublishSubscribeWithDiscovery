// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using Microsoft.ServiceBus;

namespace ServiceModelEx
{
   static class SubscriptionsStore
   {
      static List<PersistentSubscription> Store;
      const string FileName = "Subscriptions.bin";

      static bool IsSame(PersistentSubscription ps1,PersistentSubscription ps2)
      {
         return ps1.Address == ps2.Address && ps1.EventOperation == ps2.EventOperation && ps1.EventsContract == ps2.EventsContract;
      }

      static void Save(string fileName)
      {
         using(Stream stream = new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.Write))
         {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream,Store);
         }
      }
      static void Load(string fileName)
      {
         try
         {
            using(Stream stream = new FileStream(fileName,FileMode.Open,FileAccess.Read))
            {
               IFormatter formatter = new BinaryFormatter();
               Store =(List<PersistentSubscription>)formatter.Deserialize(stream);
            }
         }
         catch
         {
            Store = new List<PersistentSubscription>();
         }
      }

      public static void AddSubscription(PersistentSubscription subscription)
      {
         Load(FileName);
         foreach(PersistentSubscription ps in Store)
         {
            if(IsSame(ps,subscription))
            {
               return;
            }
         }
         Store.Add(subscription);

         Save(FileName);
      }

      public static void RemoveSubscription(PersistentSubscription subscription)
      {
         Load(FileName);
         PersistentSubscription item = null;
         foreach(PersistentSubscription ps in Store)
         {
            if(IsSame(ps,subscription))
            {
               item = ps;
            }
         }
         if(item != null)
         {
            Store.Remove(item);
         }
         Save(FileName);
      }

      public static string[] GetSubscribersToContractOperation(string eventsContract,string eventOperation)
      {
         Load(FileName);
         List<string> list = new List<string>();

         foreach(PersistentSubscription ps in Store)
         {
            if(ps.EventsContract == eventsContract && ps.EventOperation == eventOperation)
            {
               list.Add(ps.Address);
            }
         }
         return list.ToArray();
      }

      public static PersistentSubscription[] GetSubscribersToContract(string eventContract)
      {
         Load(FileName);
         List<PersistentSubscription> list = new List<PersistentSubscription>();

         foreach(PersistentSubscription ps in Store)
         {
            if(ps.EventsContract == eventContract)
            {
               list.Add(ps);
            }
         }
         return list.ToArray();
      }
      public static PersistentSubscription[] GetSubscribersFromAddress(string address)
      {
         Load(FileName);
         List<PersistentSubscription> list = new List<PersistentSubscription>();

         foreach(PersistentSubscription ps in Store)
         {
            if(ps.Address == address)
            {
               list.Add(ps);
            }
         }
         return list.ToArray();
      }

      public static PersistentSubscription[] GetAllSubscribers()
      {
         Load(FileName);
         return Store.ToArray();
      }
   }

   [BindingRequirement(TransactionFlowEnabled = true)]
   public abstract class FileSubscriptionManager<T> : SubscriptionManager<T> where T : class
   {
      //Persistent subscriptions management 
      static bool ContainsPersistent(string address,string eventsContract,string eventOperation)
      {
         string[] addresses = GetSubscribersToContractEventOperation(eventsContract,eventOperation);
         return addresses.Any(addressToMatch =>addressToMatch == address);
      }

      static void AddPersistent(string address,string eventsContract,string eventOperation)
      {
          PersistentSubscription ps = new PersistentSubscription(address,eventsContract,eventOperation);
          SubscriptionsStore.AddSubscription(ps);
      }
     
      static void RemovePersistent(string address,string eventsContract,string eventOperation)
      {
          PersistentSubscription ps = new PersistentSubscription(address, eventsContract, eventOperation);
          SubscriptionsStore.RemoveSubscription(ps);
      }
     
      static string[] GetSubscribersToContractEventOperation(string eventsContract,string eventOperation)
      {
          return SubscriptionsStore.GetSubscribersToContractOperation(eventsContract, eventOperation);
      }
     
      public static T[] GetFilePersistentList(string eventOperation)
      {
         string[] addresses =  GetSubscribersToContractEventOperation(typeof(T).ToString(),eventOperation);

         List<T> subscribers = new List<T>(addresses.Length);

         foreach(string address in addresses)
         {
            Binding binding = GetBindingFromAddress(address);
            ChannelFactory<T> factory = new ChannelFactory<T>(binding,new EndpointAddress(address));

            if(address.StartsWith("sb:"))
            {
               foreach (ServiceEndpoint endpoint in OperationContext.Current.Host.Description.Endpoints)
               {
                  if(endpoint.Address.Uri.Scheme == "sb")
                  {
                     factory.Endpoint.Behaviors.Add(endpoint.Behaviors.Find<TransportClientEndpointBehavior>());
                  }
                  break;
               }
            }

            T proxy = factory.CreateChannel();
            subscribers.Add(proxy);
         }
         return subscribers.ToArray();
      }


      [OperationBehavior(TransactionScopeRequired = true)]   
      override public PersistentSubscription[] GetAllSubscribers()
      {
         return SubscriptionsStore.GetAllSubscribers();
      }
      [OperationBehavior(TransactionScopeRequired = true)]   
      override public PersistentSubscription[] GetSubscribersToContract(string eventContract)
      {
         return SubscriptionsStore.GetSubscribersToContract(eventContract);
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      override public string[] GetSubscribersToContractEventType(string eventsContract,string eventOperation)
      {
         return GetSubscribersToContractEventOperation(eventsContract,eventOperation);
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      override public PersistentSubscription[] GetAllSubscribersFromAddress(string address)
      {
         VerifyAddress(address);

         return SubscriptionsStore.GetSubscribersFromAddress(address);
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      override public void Unsubscribe(string address,string eventsContract,string eventOperation)
      {
         VerifyAddress(address);

         if(String.IsNullOrEmpty(eventOperation) == false)
         {
            RemovePersistent(address,eventsContract,eventOperation);
         }
         else
         {
            string[] methods = GetOperations();
            Action<string> removePersistent =(methodName)=>
                                              {
                                                 RemovePersistent(address,eventsContract,methodName);
                                              };
            methods.ForEach(removePersistent);
         }
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      override public void Subscribe(string address,string eventsContract,string eventOperation)
      {
         VerifyAddress(address);

         if(String.IsNullOrEmpty(eventOperation) == false)
         {
            AddPersistent(address,eventsContract,eventOperation);
         }
         else
         {
            string[] methods = GetOperations();
            Action<string> addPersistent =(methodName)=>
                                           {
                                              AddPersistent(address,eventsContract,methodName);
                                           };
            methods.ForEach(addPersistent);
         }
      }     
   }
}