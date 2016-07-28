// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx.ServiceFabric.Actors
{
   public class ActorInstanceProvider : ActorMemoryProvider
   {
      public ActorInstanceProvider(Guid id) : base(id,new TransactionalInstanceStore<Guid,object>())
      {}
   }
}
