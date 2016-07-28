// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;


namespace ServiceModelEx.ServiceBus
{
   [ServiceContract]
   public interface IServiceBusAnnouncements
   {
      [OperationContract(IsOneWay = true)]
      void OnHello(Uri address,string contractName,string contractNamespace,Uri[] scopes);

      [OperationContract(IsOneWay = true)]
      void OnBye(Uri address,string contractName,string contractNamespace,Uri[] scopes);
   }
}
