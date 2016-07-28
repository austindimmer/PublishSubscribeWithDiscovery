// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public interface IServiceBusProperties
   {
      TransportClientEndpointBehavior Credential
      {get;set;}

      Uri[] Addresses
      {get;}
   }
}