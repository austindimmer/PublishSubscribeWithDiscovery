// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;

namespace ServiceModelEx.TcpDiscovery
{
   [ServiceContract(CallbackContract = typeof(IDiscoveryCallback))]
   public interface IDiscovery
   {
      [OperationContract(IsOneWay = true)]
      void OnDiscoveryRequest(string contractName,string contractNamespace,Uri[] scopesToMatch);
   }
}
   
 