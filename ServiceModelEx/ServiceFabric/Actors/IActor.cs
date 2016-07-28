// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [ServiceContract(SessionMode = SessionMode.Required)]
   public interface IActor
   {
      ActorId Id
      {get;}

      /// <summary>
      /// For internal ServiceModelEx.ServiceFabric use only. DO NOT USE!
      /// </summary>
      [OperationContract]
      Task ActivateAsync();

      /// <summary>
      /// For internal ServiceModelEx.ServiceFabric use only. DO NOT USE!
      /// </summary>
      [OperationContract(IsInitiating = false)]
      Task DeactivateAsync();
   }
}
