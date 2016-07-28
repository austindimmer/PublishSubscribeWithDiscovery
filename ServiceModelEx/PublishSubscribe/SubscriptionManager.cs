// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ServiceModelEx.PublishSubscribe;
using ServiceModelEx.PublishSubscribe.PublishSubscribeDataSetTableAdapters;
using System.ServiceModel.Description;

using Microsoft.ServiceBus;


namespace ServiceModelEx
{
   [BindingRequirement(TransactionFlowEnabled = true)]
   public abstract class SubscriptionManager<T> where T : class
   {
      static Dictionary<string,List<T>> m_TransientStore;

      static SubscriptionManager()
      {
         m_TransientStore = new Dictionary<string, List<T>>();
         string[] methods = GetOperations();
         Action<string> insert = (methodName)=>
                                 {
                                    m_TransientStore.Add(methodName,new List<T>());
                                 };
         methods.ForEach(insert);
      }
      
      //Helper methods 
      static protected void VerifyAddress(string address)
      {
         if(address.StartsWith("http:") || address.StartsWith("https:"))
         {
            return;
         }
         if(address.StartsWith("net.tcp:"))
         {
            return;
         }
         if(address.StartsWith("net.pipe:"))
         {
            return;
         }
         if(address.StartsWith("net.msmq:"))
         {
            return;
         }
         if(address.StartsWith("sb:"))
         {
            return;
         }

         throw new InvalidOperationException("Unsupported protocol specified");
      }
      static protected Binding GetBindingFromAddress(string address)
      {
         if(address.StartsWith("http:") || address.StartsWith("https:"))
         {
            WSHttpBinding binding = new WSHttpBinding();
            binding.ReliableSession.Enabled = true;
            binding.TransactionFlow = true;
            return binding;
         }
         if(address.StartsWith("net.tcp:"))
         {
            NetTcpBinding binding = new NetTcpBinding();
            binding.ReliableSession.Enabled = true;
            binding.TransactionFlow = true;
            return binding;
         }
         if(address.StartsWith("net.pipe:"))
         {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.TransactionFlow = true;
            return binding;
         }
         if(address.StartsWith("net.msmq:"))
         {
            NetMsmqBinding binding = new NetMsmqBinding();
            binding.Security.Mode = NetMsmqSecurityMode.None; 
            return binding;
         }
         if(address.StartsWith("sb:"))
         {
            NetOnewayRelayBinding binding = new NetOnewayRelayBinding();
            return binding;
         }

         Debug.Assert(false,"Unsupported binding specified");
         return null;
      }
      static protected string[] GetOperations()
      {
         MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Public|BindingFlags.FlattenHierarchy|BindingFlags.Instance);
         List<string> operations = new List<string>(methods.Length);

         Action<MethodInfo> add = (method)=>
                                  {
                                     Debug.Assert(! operations.Contains(method.Name));
                                     operations.Add(method.Name);
                                  };
         methods.ForEach(add);
         return operations.ToArray();
      }

      //Transient subscriptions management 
      internal static T[] GetTransientList(string eventOperation)
      {
         lock(typeof(SubscriptionManager<T>))
         {
            Debug.Assert(m_TransientStore.ContainsKey(eventOperation));
            if(m_TransientStore.ContainsKey(eventOperation))
            {
               List<T> list = m_TransientStore[eventOperation];
               return list.ToArray();
            }
            return new T[]{};
         }
      }
      static void AddTransient(T subscriber,string eventOperation)
      {
         lock(typeof(SubscriptionManager<T>))
         {
            List<T> list = m_TransientStore[eventOperation];
            if(list.Contains(subscriber))
            {
               return;
            }
            list.Add(subscriber);
         }
      }
      static void RemoveTransient(T subscriber,string eventOperation)
      {
         lock(typeof(SubscriptionManager<T>))
         {
            List<T> list = m_TransientStore[eventOperation];
            list.Remove(subscriber);
         }
      }

      public void Subscribe(string eventOperation)
      {
         lock(typeof(SubscriptionManager<T>))
         {
            T subscriber = OperationContext.Current.GetCallbackChannel<T>();
            if(String.IsNullOrEmpty(eventOperation) == false)
            {
               AddTransient(subscriber,eventOperation);
            }
            else
            {
               string[] methods = GetOperations();
               Action<string> addTransient = (methodName)=>
                                             {
                                                AddTransient(subscriber,methodName);
                                             };
               methods.ForEach(addTransient);
            }
         }
      }
      
      public void Unsubscribe(string eventOperation)
      {
         lock(typeof(SubscriptionManager<T>))
         {
            T subscriber = OperationContext.Current.GetCallbackChannel<T>();
            if(String.IsNullOrEmpty(eventOperation) == false)
            {
               RemoveTransient(subscriber,eventOperation);
            }
            else
            {
               string[] methods = GetOperations();
               Action<string> removeTransient = (methodName)=>
                                                {
                                                   RemoveTransient(subscriber,methodName);
                                                };
               methods.ForEach(removeTransient);
            }
         }
      }      
      
      //Persistent subscriptions management 
      static bool ContainsPersistent(string address,string eventsContract,string eventOperation)
      {
         string[] addresses = GetSubscribersToContractEventOperation(eventsContract,eventOperation);
         return addresses.Any(addressToMatch =>addressToMatch == address);
      }

      static void AddPersistent(string address,string eventsContract,string eventOperation)
      {
         bool exists = ContainsPersistent(address,eventsContract,eventOperation);
         if(exists)
         {
            return;
         }
         PersistentSubscribersTableAdapter adapter = new PersistentSubscribersTableAdapter();
         adapter.Insert(address,eventOperation,eventsContract);
      }
     
      static void RemovePersistent(string address,string eventsContract,string eventOperation)
      {
         PersistentSubscribersTableAdapter adapter = new PersistentSubscribersTableAdapter();

         PublishSubscribeDataSet.PersistentSubscribersDataTable subscribers = adapter.GetSubscribersByAddressContractOperation(address,eventsContract,eventOperation);
         foreach(PublishSubscribeDataSet.PersistentSubscribersRow subscriber in subscribers)
         {
            adapter.Delete(subscriber.Address,subscriber.Operation,subscriber.Contract,subscriber.ID);
         }
      }
      
      static PersistentSubscription[] Convert(PublishSubscribeDataSet.PersistentSubscribersDataTable subscribers)
      {
         Func<PublishSubscribeDataSet.PersistentSubscribersRow,PersistentSubscription> converter;
         converter = (row)=>
                     {
                        return new PersistentSubscription()
                                   {
                                      Address = row.Address,
                                      EventsContract = row.Contract,
                                      EventOperation = row.Operation
                                   };
                     };
         return subscribers.ToArray(converter);
      } 
      internal static T[] GetPersistentList(string eventOperation)
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
      
      static string[] GetSubscribersToContractEventOperation(string eventsContract,string eventOperation)
      {
         PublishSubscribeDataSet.PersistentSubscribersDataTable subscribers = new PublishSubscribeDataSet.PersistentSubscribersDataTable();
         PersistentSubscribersTableAdapter adapter = new PersistentSubscribersTableAdapter();
         subscribers = adapter.GetSubscribersToContractOperation(eventsContract,eventOperation);

         Func<PublishSubscribeDataSet.PersistentSubscribersRow,string> extract = (row)=>
                                                                                 {
                                                                                    return row.Address;
                                                                                 };
         return subscribers.ToArray(extract);
      }
     
      [OperationBehavior(TransactionScopeRequired = true)]   
      virtual public PersistentSubscription[] GetAllSubscribers()
      {
         PublishSubscribeDataSet.PersistentSubscribersDataTable subscribers = new PublishSubscribeDataSet.PersistentSubscribersDataTable();
         PersistentSubscribersTableAdapter adapter = new PersistentSubscribersTableAdapter();
         subscribers = adapter.GetAllSubscribers();
         return Convert(subscribers);
      }
      [OperationBehavior(TransactionScopeRequired = true)]   
      virtual public PersistentSubscription[] GetSubscribersToContract(string eventContract)
      {
         PublishSubscribeDataSet.PersistentSubscribersDataTable subscribers = new PublishSubscribeDataSet.PersistentSubscribersDataTable();
         PersistentSubscribersTableAdapter adapter = new PersistentSubscribersTableAdapter();
         subscribers = adapter.GetSubscribersToContract(eventContract);
         return Convert(subscribers);
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      virtual public string[] GetSubscribersToContractEventType(string eventsContract,string eventOperation)
      {
         return GetSubscribersToContractEventOperation(eventsContract,eventOperation);
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      virtual public PersistentSubscription[] GetAllSubscribersFromAddress(string address)
      {
         VerifyAddress(address);

         PublishSubscribeDataSet.PersistentSubscribersDataTable subscribers = new PublishSubscribeDataSet.PersistentSubscribersDataTable();

         PersistentSubscribersTableAdapter adapter = new PersistentSubscribersTableAdapter();
         subscribers = adapter.GetSubscribersFromAddress(address);

         return Convert(subscribers);
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      virtual public void Unsubscribe(string address,string eventsContract,string eventOperation)
      {
         VerifyAddress(address);

         if(String.IsNullOrEmpty(eventOperation) == false)
         {
            RemovePersistent(address,eventsContract,eventOperation);
         }
         else
         {
            string[] methods = GetOperations();
            Action<string> removePersistent = (methodName)=>
                                              {
                                                 RemovePersistent(address,eventsContract,methodName);
                                              };
            methods.ForEach(removePersistent);
         }
      }
      [OperationBehavior(TransactionScopeRequired = true)]
      virtual public void Subscribe(string address,string eventsContract,string eventOperation)
      {
         VerifyAddress(address);

         if(String.IsNullOrEmpty(eventOperation) == false)
         {
            AddPersistent(address,eventsContract,eventOperation);
         }
         else
         {
            string[] methods = GetOperations();
            Action<string> addPersistent = (methodName)=>
                                           {
                                              AddPersistent(address,eventsContract,methodName);
                                           };
            methods.ForEach(addPersistent);
         }
      }     
   }
}