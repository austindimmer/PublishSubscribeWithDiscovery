// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.ServiceModel.Persistence;

#pragma warning disable 618

namespace ServiceModelEx
{
   public abstract class MemoryProviderFactory : PersistenceProviderFactory
   {
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

      protected override void OnClose(TimeSpan timeout)
      {}

      protected override void OnEndClose(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      protected override void OnEndOpen(IAsyncResult result)
      {
         throw new NotImplementedException();
      }

      protected override void OnOpen(TimeSpan timeout)
      {}
   }
}

#pragma warning restore 618