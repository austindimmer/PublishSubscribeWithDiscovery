// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [AttributeUsage(AttributeTargets.Method)]
   public class CompletesActorInstanceAttribute : Attribute
   {}
}
