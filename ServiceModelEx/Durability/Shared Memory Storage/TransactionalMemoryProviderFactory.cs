// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.ServiceModel.Persistence;

#pragma warning disable 618

namespace ServiceModelEx
{
   public class TransactionalMemoryProviderFactory : MemoryProviderFactory
   {
      public override PersistenceProvider CreateProvider(Guid id)
      {
         return new TransactionalMemoryProvider(id);
      }
   }
}

#pragma warning restore 618