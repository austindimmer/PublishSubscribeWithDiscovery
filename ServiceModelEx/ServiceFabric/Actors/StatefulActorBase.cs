// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Actors
{
   [Serializable]
   public abstract class StatefulActor<S> : ActorBase,IStatefulActorManagement where S : class,new()
   {
      public bool Completing
      {get; private set;}
      protected S State
      {get;set;}

      protected StatefulActor()
      {
         State = new S();
      }

      protected virtual Task OnCompleteAsync()
      {
         return Task.FromResult(true);
      }
      public async Task CompleteAsync()
      {
         ActorId actorId = GenericContext<ActorId>.Current.Value;

         await OnCompleteAsync().FlowWcfContext();

         Completing = true;
         OperationContext.Current.InstanceContext.Extensions.Add(new ActorInstanceContextProvider.ActorCompleted(actorId));
         DurableOperationContext.CompleteInstance();
         //Manage ActorIds here so file access failure will also abort the transaction.
         ActorManager.RemoveInstance(actorId);
      }
   }
}

#pragma warning restore 618
