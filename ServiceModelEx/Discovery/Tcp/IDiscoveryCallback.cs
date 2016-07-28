// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;

namespace ServiceModelEx.TcpDiscovery
{
   [ServiceContract]
   public interface IDiscoveryCallback
   {
      [OperationContract(IsOneWay = true)]
      void OnDiscoveryResponse(Uri address,string contractName,string contractNamespace,Uri[] scopes);
   }
}