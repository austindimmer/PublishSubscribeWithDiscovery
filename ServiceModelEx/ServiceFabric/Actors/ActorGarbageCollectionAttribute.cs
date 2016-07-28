// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [Serializable]
   [AttributeUsage(AttributeTargets.Class)] 
   public class ActorGarbageCollectionAttribute : Attribute
   {
      public long IdleTimeoutInSeconds 
      {get; set;}
      public long ScanIntervalInSeconds 
      {get; set;}
      public ActorGarbageCollectionAttribute()
      {
         IdleTimeoutInSeconds  = 3600;
         ScanIntervalInSeconds = 60;
      }
   }
}
