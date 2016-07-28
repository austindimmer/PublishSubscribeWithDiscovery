// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using ServiceModelEx.ServiceFabric.Services.Client;

namespace ServiceModelEx.ServiceFabric.Services.Communication.Client
{
   public interface ICommunicationClientFactory<T> where T : ICommunicationClient
   {
      ServicePartitionResolver ServiceResolver
      {get;}
      NetTcpBinding Binding
      {get;}
      T GetClient(string applicationName,string serviceName);
   }
}
