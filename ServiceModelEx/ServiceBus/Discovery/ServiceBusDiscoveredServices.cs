// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel.Discovery;
using ServiceModelEx.ServiceBus;

namespace ServiceModelEx
{
   public class ServiceBusDiscoveredServices<T> : DiscoveredServices<T> where T : class
   {
      string m_Owner;
      string m_Secret;
      string m_ServiceNamespace;

      public ServiceBusDiscoveredServices(string serviceNamespace,string secret,ServiceBusDiscoveredServices<T> container = null) : this(serviceNamespace,ServiceBusHelper.DefaultIssuer,secret,container)
      {}
      public ServiceBusDiscoveredServices(string serviceNamespace,string owner,string secret,ServiceBusDiscoveredServices<T> container = null) : base(container)
      {
         m_ServiceNamespace = serviceNamespace;
         m_Owner = owner;
         m_Secret = secret;
      }

      override protected FindResponse Find()
      {
         ServiceBusDiscoveryClient discoveryClient = new ServiceBusDiscoveryClient(m_ServiceNamespace,m_Secret);
         FindCriteria criteria = new FindCriteria(typeof(T));
         FindResponse response = discoveryClient.Find(criteria);
         discoveryClient.Close();
         return response;
      }
   }
}


