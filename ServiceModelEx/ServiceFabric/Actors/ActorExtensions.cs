// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

namespace ServiceModelEx.ServiceFabric.Actors
{
   public static class ActorExtensions
   {
      public static ActorId GetActorId<I>(this I actor) where I : class,IActor
      {
         ActorId actorId = null;
         if(actor is ActorBase)
         {
            actorId = (actor as ActorBase).Id;
         }
         else
         {
            actorId = actor.Id;
         }
         return actorId;
      }
   }
}
