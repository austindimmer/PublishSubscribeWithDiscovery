// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [ServiceContract(SessionMode = SessionMode.Required)]
   internal interface IStatefulActorManagement : IActor
   {
      bool Completing
      {get;}

      [OperationContract(IsInitiating = false)]
      Task CompleteAsync();
   }
}
