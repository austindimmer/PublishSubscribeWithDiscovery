// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;

namespace ServiceModelEx.TcpDiscovery
{
   [ServiceContract(CallbackContract = typeof(IAnnouncements))]
   public interface IAnnouncementsSubscription
   {
      [OperationContract]
      void Subscribe();

      [OperationContract]
      void Unsubscribe();
   }
}