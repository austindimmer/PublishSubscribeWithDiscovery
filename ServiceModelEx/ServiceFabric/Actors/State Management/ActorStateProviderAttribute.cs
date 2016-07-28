// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
   public abstract class ActorStateProviderAttribute : Attribute
   {}
}
