// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;

namespace ServiceModelEx
{
   public class Transactional<T> : ISinglePhaseNotification 
   {
      T m_Value;
      T m_TemporaryValue;
      Transaction m_CurrentTransaction; 
      TransactionalLock m_Lock;

      public Transactional(T value)
      {
         m_Lock = new TransactionalLock();
         m_Value = value;
      }
      public Transactional(Transactional<T> transactional) : this(transactional.Value)
      {}
      public Transactional() : this(default(T))
      {}
      static Transactional()
      {
         ResourceManager.ConstrainType(typeof(T));
      }
      void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
      {
         Commit();
         singlePhaseEnlistment.Committed();
      }
      void Commit()
      {
         IDisposable disposable = m_Value as IDisposable;
         if(disposable != null)
         {
            disposable.Dispose();
         }
         m_Value = m_TemporaryValue;
         m_CurrentTransaction = null;
         m_TemporaryValue= default(T);
         m_Lock.Unlock();      
      }
      void IEnlistmentNotification.Commit(Enlistment enlistment)
      {
         Commit();
         enlistment.Done();
      }
      
      void IEnlistmentNotification.InDoubt(Enlistment enlistment)
      {
         m_Lock.Unlock();
         enlistment.Done();
      }
      void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
      {
         preparingEnlistment.Prepared();
      }

      void IEnlistmentNotification.Rollback(Enlistment enlistment)
      {
         m_CurrentTransaction = null;

         IDisposable disposable = m_TemporaryValue as IDisposable;
         if(disposable != null)
         {
            disposable.Dispose();
         }

         m_TemporaryValue = default(T);
         m_Lock.Unlock();
         enlistment.Done();
      }
      void Enlist(T t)
      {
         Debug.Assert(m_CurrentTransaction == null);
         m_CurrentTransaction = Transaction.Current;
         Debug.Assert(m_CurrentTransaction.TransactionInformation.Status == TransactionStatus.Active);
         m_CurrentTransaction.EnlistVolatile(this,EnlistmentOptions.None);
         m_TemporaryValue = ResourceManager.Clone(t);
      }
      void SetValue(T t)
      {
         m_Lock.Lock();
         if(m_CurrentTransaction == null)
         {
            if(Transaction.Current == null)
            {
               m_Value = t;
               return;
            }
            else
            {
               Enlist(t);
               return;
            }
         }
         else
         {
            //Must have acquired the lock
            Debug.Assert(m_CurrentTransaction == Transaction.Current,"Invalid state in the volatile resource state machine");
            m_TemporaryValue = t;
         }
      }
      T GetValue()
      {
         m_Lock.Lock();
         if(m_CurrentTransaction == null)
         {
            if(Transaction.Current == null)
            {
               return m_Value;
            }
            else
            {
               Enlist(m_Value); 
            }
         }
         //Must have acquired the lock
         Debug.Assert(m_CurrentTransaction == Transaction.Current,"Invalid state in the volatile resource state machine");

         return m_TemporaryValue; 
      }
      public T Value
      {
         get
         {
            return GetValue();
         }
         set
         {
            SetValue(value);
         }
      }
      public static implicit operator T(Transactional<T> transactional)
      {
         return transactional.Value;
      }
      public static bool operator==(Transactional<T> t1,Transactional<T> t2)
      {
         // Is t1 and t2 null (check the value as well).
         bool t1Null = (Object.ReferenceEquals(t1,null) || t1.Value == null);
         bool t2Null = (Object.ReferenceEquals(t2,null) || t2.Value == null);

         // If they are both null,return true.
         if(t1Null && t2Null)
         {
            return true;
         }

         // If one is null,return false.
         if(t1Null || t2Null)
         {
            return false;
         }
         return EqualityComparer<T>.Default.Equals(t1.Value,t2.Value);
      }
      public static bool operator==(Transactional<T> t1,T t2)
      {
         // Is t1 and t2 null (check the value as well).
         bool t1Null = (Object.ReferenceEquals(t1,null) || t1.Value == null);
         bool t2Null = t2 == null;

         // If they are both null,return true.
         if(t1Null && t2Null)
         {
            return true;
         }
   
         // If one is null,return false.
         if(t1Null || t2Null)
         {
            return false;
         }
         return EqualityComparer<T>.Default.Equals(t1.Value,t2);
      }
      public static bool operator==(T t1,Transactional<T> t2)
      {
         // Is t1 and t2 null (check the value as well)
         bool t1Null = t1 == null;
         bool t2Null = (Object.ReferenceEquals(t2,null) || t2.Value == null);

         // If they are both null,return true.
         if(t1Null && t2Null)
         {
            return true;
         }

         // If one is null,return false.
         if(t1Null || t2Null)
         {
            return false;
         }
         return EqualityComparer<T>.Default.Equals(t1,t2.Value);
      }
      public static bool operator!=(T t1,Transactional<T> t2)
      {
         return ! (t1 == t2);
      }
      public static bool operator!=(Transactional<T> t1,T t2)
      {
         return !(t1 == t2);
      }
      public static bool operator!=(Transactional<T> t1,Transactional<T> t2)
      {
         return !(t1 == t2);
      }
      public override int GetHashCode()
      {
         return Value.GetHashCode();
      }
      public override bool Equals(object obj)
      {
         return Value.Equals(obj);
      }
   }
}
