// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Transactions;

namespace ServiceModelEx
{
   /// <summary>
   /// Protects a resource manager by proving exclusive transaction isolation (Serializable level only)
   /// </summary>
   public class TransactionalLock
   {
      //Transaction that tried to acquire the lock while the lock is owned by another transaction are placed in this queue
      LinkedList<KeyValuePair<Transaction,ManualResetEvent>> m_PendingTransactions = new LinkedList<KeyValuePair<Transaction,ManualResetEvent>>();
      Transaction m_OwningTransaction;

      Transaction OwningTransaction
      {
         get 
         {
            lock(this)
            {
               return m_OwningTransaction;
            }
         }
         set 
         {
            lock(this)
            {
               m_OwningTransaction = value;
            }
         }
      }
      public bool Locked
      {
         get
         {
            return OwningTransaction != null;
         }
      }      
      /// <summary>
      /// Acquires the lock for the exclusive use of a transaction. If another transaction owns the lock,it blocks the calling transaction and places it in a queue. If the transaction owns the lock already Lock() does nothing. 
      /// </summary>
      public void Lock()
      {
         Lock(Transaction.Current);
      }
      void Lock(Transaction transaction)
      {
         bool taken = false;

         Monitor.Enter(this,ref taken);

         Debug.Assert(taken);

         if(OwningTransaction == null)
         {
            if(transaction == null)
            {
               Monitor.Exit(this); 
               return;
            }
            else
            {
               Debug.Assert(transaction.IsolationLevel == IsolationLevel.Serializable);

               //Acquire the lock
               OwningTransaction = transaction;
               Monitor.Exit(this); 
               return;
            }
         }
         else //Some transaction owns the lock
         {
            //Is it the same one?
            if(OwningTransaction == transaction)
            {
               Monitor.Exit(this); 
               return;
            }
            else //Need to lock
            {
               ManualResetEvent manualEvent = new ManualResetEvent(false);

               KeyValuePair<Transaction,ManualResetEvent> pair;
               pair = new KeyValuePair<Transaction,ManualResetEvent>(transaction,manualEvent);
               m_PendingTransactions.AddLast(pair);

               if(transaction != null)
               {
                  Debug.Assert(transaction.TransactionInformation.Status == TransactionStatus.Active);
                  //Since the transaction can abort or just time out while blocking,unblock it when it is completed and remove from the queue
                  transaction.TransactionCompleted += delegate
                                                      {
                                                         lock(this)
                                                         {
                                                            //Note that the pair may have already been removed if unlocked
                                                            m_PendingTransactions.Remove(pair);
                                                         }
                                                         lock(manualEvent)//To deal with race condition of the handle closed between the check and the set
                                                         {
                                                            if(manualEvent.SafeWaitHandle.IsClosed == false)
                                                            {
                                                               manualEvent.Set();
                                                            }
                                                         }
                                                      };
               }
               Monitor.Exit(this); 
               //Block the transaction or the calling thread
               manualEvent.WaitOne();
               lock(manualEvent)//To deal with race condition of the other threads setting the handle
               {
                  manualEvent.Close();
               }
            }
         }
      }
      //Releases the transaction lock and allows the next pending transaction to quire it. 
      public void Unlock()
      {
         Debug.Assert(Locked);
         lock(this)
         {
            OwningTransaction = null;
                  
            LinkedListNode<KeyValuePair<Transaction,ManualResetEvent>> node = null;

            if(m_PendingTransactions.Count > 0)
            {
               node = m_PendingTransactions.First;
               m_PendingTransactions.RemoveFirst();
            }
            if(node != null)
            {
               Transaction transaction = node.Value.Key;
               ManualResetEvent manualEvent = node.Value.Value;
               Lock(transaction);
               lock(manualEvent)//To deal with race condition of the handle closed between the check and the set
               {
                  if(manualEvent.SafeWaitHandle.IsClosed == false)
                  {
                     manualEvent.Set();
                  }
               }
            }
         }
      }
   }
}
