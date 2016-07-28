// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public abstract class ServiceBusEventsClientBase<T> : OneWayClientBase<T> where T : class
   {
      public ServiceBusEventsClientBase(string namespaceBaseAddress,string username,string password) : this(namespaceBaseAddress,new NetOnewayRelayBinding(),username,password)
      {}
      public ServiceBusEventsClientBase(string namespaceBaseAddress,NetOnewayRelayBinding binding,string username,string password) : base(binding,ToEventAddress(namespaceBaseAddress),username,password)
      {}
      public ServiceBusEventsClientBase(string namespaceBaseAddress) : this(namespaceBaseAddress,new NetOnewayRelayBinding())
      {}      
      public ServiceBusEventsClientBase(string namespaceBaseAddress,NetOnewayRelayBinding binding) : base(binding,ToEventAddress(namespaceBaseAddress))
      {}
      static EndpointAddress ToEventAddress(string namespaceBaseAddress)
      {
         if(namespaceBaseAddress.EndsWith("/") == false)
         {
            namespaceBaseAddress += "/";
         }
         return new EndpointAddress(namespaceBaseAddress + typeof(T).Name + "/");
      }
   }
}