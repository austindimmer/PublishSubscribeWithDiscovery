// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Linq;
using System.Reflection;

using ServiceModelEx.Fabric;

namespace ServiceModelEx.ServiceFabric.Actors
{
   public static class ActorRegistration
   {
      public static void RegisterActor<T>(this FabricRuntime fabricRuntime)
      {
         Type actorType = typeof(T);
         if(actorType.GetInterfaces().Count(contract=>!contract.Namespace.Contains("ServiceModelEx")) > 1)
         {
            throw new InvalidOperationException("Validation failed. Multiple application interfaces found. An actor may only possess a single application interface.");
         }
         else if(FabricRuntime.Actors.Count > 0)
         {
            Type actorContract = actorType.GetInterfaces().Single(contract=>!contract.Namespace.Contains("ServiceModelEx")); 

            string[] applications = actorType.GetCustomAttributes<ApplicationManifestAttribute>().Select(manifest=>manifest.ApplicationName).ToArray();
            foreach(string application in applications)
            {
               if(FabricRuntime.Actors.ContainsKey(application) &&
                   FabricRuntime.Actors[application].Any(type=>type.GetInterfaces().Any(contract=>contract.Equals(actorContract))))
               {
                  throw new InvalidOperationException("Validation failed. Actor interface " + actorContract.Name + " already exists within application " + application + " . An Actor interface must be unique within an application.");
               }
            }
         }
         fabricRuntime.RegisterServiceType(FabricRuntime.Actors,actorType);
      }
   }
}
