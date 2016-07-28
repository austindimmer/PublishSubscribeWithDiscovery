// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServiceModelEx
{
   //For transient subscribers
   [ServiceContract]
   public interface ISubscriptionService
   {
      [OperationContract]
      void Subscribe(string eventOperation);

      [OperationContract]
      void Unsubscribe(string eventOperation);
   }

   //For persistent subscribers
   [Serializable]
   public class PersistentSubscription
   {
      public PersistentSubscription()
      {}

      public PersistentSubscription(string address,string contract,string operation)
      {
         Address = address;
         EventsContract = contract;
         EventOperation = operation;
      }


      [DataMember]
      public string Address
      {get;set;}

      [DataMember]
      public string EventsContract
      {get;set;}

      [DataMember]
      public string EventOperation
      {get;set;}
   }

   [ServiceContract]
   public interface IPersistentSubscriptionService
   {
      [OperationContract]
      [TransactionFlow(TransactionFlowOption.Allowed)]
      void Subscribe(string address,string eventsContract,string eventOperation);

      [OperationContract]
      [TransactionFlow(TransactionFlowOption.Allowed)]
      void Unsubscribe(string address,string eventsContract,string eventOperation);

      [OperationContract]
      [TransactionFlow(TransactionFlowOption.Allowed)]
      PersistentSubscription[] GetAllSubscribers();

      [OperationContract]
      [TransactionFlow(TransactionFlowOption.Allowed)]
      PersistentSubscription[] GetSubscribersToContract(string eventsContract);

      [OperationContract]
      [TransactionFlow(TransactionFlowOption.Allowed)]
      string[] GetSubscribersToContractEventType(string eventsContract,string eventOperation);

      [OperationContract]
      [TransactionFlow(TransactionFlowOption.Allowed)]
      PersistentSubscription[] GetAllSubscribersFromAddress(string address);
   }
}