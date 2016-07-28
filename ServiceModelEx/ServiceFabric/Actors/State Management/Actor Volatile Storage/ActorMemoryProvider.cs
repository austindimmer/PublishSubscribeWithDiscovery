// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel.Persistence;
using System.Threading;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Actors
{
   public abstract class ActorMemoryProvider : PersistenceProvider
   {
      IInstanceStore<Guid,object> m_InstanceStore;

      public ActorMemoryProvider(Guid id,IInstanceStore<Guid,object> instanceStore) : base(id)
      {
         m_InstanceStore = instanceStore;
      }

      protected override void OnOpen(TimeSpan timeout)
      {}
      protected override void OnClose(TimeSpan timeout)
      {
         throw new NotImplementedException();
      }
      protected override void OnAbort()
      {}

      public override object Create(object instance,TimeSpan timeout)
      {
         m_InstanceStore[Id] = instance;
         return null;
      }
      public override object Load(TimeSpan timeout)
      {
         if(m_InstanceStore.ContainsInstance(Id))
         {
            return m_InstanceStore[Id];
         }
         return null;
      }
      public override void Delete(object instance,TimeSpan timeout)
      {
         //We are controlling state unload through InstanceContext mgmt.
         if((instance as IStatefulActorManagement).Completing)
         {
            m_InstanceStore.RemoveInstance(Id);
         }
      }
      public override object Update(object instance,TimeSpan timeout)
      {
         m_InstanceStore[Id] = instance;
         return null;
      }

      protected override TimeSpan DefaultCloseTimeout
      {
         get
         {
            return TimeSpan.MaxValue;
         }
      }
      protected override TimeSpan DefaultOpenTimeout
      {
         get
         {
            return TimeSpan.MaxValue;
         }
      }

      object End(IAsyncResult result)
      {
         return (result as AsyncResult).ReturnValue;
      }

      public override IAsyncResult BeginCreate(object instance,TimeSpan timeout,AsyncCallback callback,object state)
      {
         object value = Create(instance,timeout);
         AsyncResult result = new AsyncResult(state,value);
         return result;
      }
      public override object EndCreate(IAsyncResult result)
      {
         return End(result);
      }

      public override IAsyncResult BeginLoad(TimeSpan timeout,AsyncCallback callback,object state)
      {
         object value = Load(timeout);
         AsyncResult result = new AsyncResult(state,value);
         return result;
      }
      public override object EndLoad(IAsyncResult result)
      {
         return End(result);
      }

      public override IAsyncResult BeginUpdate(object instance,TimeSpan timeout,AsyncCallback callback,object state)
      {
         object value = Update(instance,timeout);
         AsyncResult result = new AsyncResult(state,value);
         return result;
      }
      public override object EndUpdate(IAsyncResult result)
      {
         return End(result);
      }

      public override IAsyncResult BeginDelete(object instance,TimeSpan timeout,AsyncCallback callback,object state)
      {
         Delete(instance,timeout);
         AsyncResult result = new AsyncResult(state,null);
         return result;
      }
      public override void EndDelete(IAsyncResult result)
      {
         End(result);
      }

      protected override IAsyncResult OnBeginClose(TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }
      protected override void OnEndClose(IAsyncResult result)
      {
         throw new NotImplementedException();
      }
      protected override IAsyncResult OnBeginOpen(TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }
      protected override void OnEndOpen(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      class AsyncResult : IAsyncResult
      {
         ManualResetEvent m_AsyncWaitHandle;
         readonly object m_State;
         readonly object m_ReturnValue;

         public AsyncResult(object state,object returnValue)
         {
            m_State = state;
            m_ReturnValue = returnValue;
         }

         public object AsyncState
         {
            get
            {
               return m_State;
            }
         }
         public System.Threading.WaitHandle AsyncWaitHandle
         {
            get
            {
               if(m_AsyncWaitHandle == null)
               {
                  m_AsyncWaitHandle = new ManualResetEvent(true);
               }
               return m_AsyncWaitHandle;
            }
         }
         public bool CompletedSynchronously
         {
            get
            {
               return true;
            }
         }
         public bool IsCompleted
         {
            get
            {
               return true;
            }
         }
         public object ReturnValue
         {
            get
            {
               return m_ReturnValue;
            }
         }
      }
   }
}
#pragma warning restore 618