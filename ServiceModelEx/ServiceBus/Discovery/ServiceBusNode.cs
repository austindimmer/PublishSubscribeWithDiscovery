// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public partial class ServiceBusNode
   {
      public readonly string Address;

      public string Name
      {get;set;}

      public ServiceBusNode(string address)
      {
         Address = address;
         Name = AddressToName(address);
      }

      static string AddressToName(string address)
      {
         if(String.IsNullOrEmpty(address))
         {
            return address;
         }
         Uri uri = new Uri(address);

         string localPath = uri.LocalPath;
         if(localPath.StartsWith("/"))
         {
            localPath = localPath.Remove(0,1);
         }
         if(localPath.EndsWith("/"))
         {
            localPath = localPath.Remove(localPath.Length-1,1);
         }
         return localPath;
      }      
   }
}