// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel.Persistence;

#pragma warning disable 618

namespace ServiceModelEx
{
   public abstract class MemoryProvider : PersistenceProvider
   {
      IInstanceStore<Guid,object> m_InstanceStore;

      public MemoryProvider(Guid id,IInstanceStore<Guid,object> instanceStore) : base(id)
      {
         m_InstanceStore = instanceStore;
      }

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
         m_InstanceStore.RemoveInstance(Id);
      }
      protected override void OnClose(TimeSpan timeout)
      {
         throw new NotImplementedException();
      }
      protected override void OnOpen(TimeSpan timeout)
      {}
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
      public override IAsyncResult BeginCreate(object instance,TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }

      public override IAsyncResult BeginDelete(object instance,TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }

      public override IAsyncResult BeginLoad(TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }

      public override IAsyncResult BeginUpdate(object instance,TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }

      public override object EndCreate(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      public override void EndDelete(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      public override object EndLoad(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      public override object EndUpdate(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      protected override void OnAbort()
      {}

      protected override IAsyncResult OnBeginClose(TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }

      protected override IAsyncResult OnBeginOpen(TimeSpan timeout,AsyncCallback callback,object state)
      {
         throw new NotImplementedException();
      }

      protected override void OnEndClose(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      protected override void OnEndOpen(IAsyncResult result)
      {
         throw new NotImplementedException();
      }
   }
}
#pragma warning restore 618