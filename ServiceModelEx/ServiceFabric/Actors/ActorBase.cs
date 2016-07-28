// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [Serializable]
   [ActorErrorHandlerBehavior]
   public class ActorBase : IActor
   {
      public string ApplicationName
      {get;private set;}
      public ActorId Id
      {get;private set;}
      public Uri ServiceUri
      {get;private set;}

      internal bool Activated
      {get;private set;}
      internal void Activate()
      {
         Activated = true;
         Id = GenericContext<ActorId>.Current.Value;

         Debug.Assert(!string.IsNullOrEmpty(Id.ApplicationName));
         ApplicationName = Id.ApplicationName;

         ApplicationManifestAttribute appManifest = this.GetType().GetCustomAttributes<ApplicationManifestAttribute>().SingleOrDefault(manifest=>manifest.ApplicationName.Equals(ApplicationName));
         Debug.Assert(appManifest != null);
         Debug.Assert(appManifest.ApplicationName.Equals(ApplicationName));
         ServiceUri = new Uri("fabric:/" + appManifest.ApplicationName + "/" + appManifest.ServiceName);
      }

      protected virtual Task OnActivateAsync()
      {
         return Task.FromResult(true);
      }
      protected virtual Task OnDeactivateAsync()
      {
         return Task.FromResult(true);
      }

      public async Task ActivateAsync()
      {  
         if(!Activated)
         {
            Activate();
            await OnActivateAsync().FlowWcfContext();
         }
      }
      public async Task DeactivateAsync()
      {
         if(Activated)
         {
            await OnDeactivateAsync().FlowWcfContext();
            Activated = false;
         }
      }
   }
}
