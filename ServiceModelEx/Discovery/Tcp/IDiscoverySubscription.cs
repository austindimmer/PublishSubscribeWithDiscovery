// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;

namespace ServiceModelEx.TcpDiscovery
{
   [ServiceContract]
   public interface IDiscoverySubscription
   {
      [OperationContract]
      void Subscribe(string address);

      [OperationContract]
      void Unsubscribe(string address);
   }
}