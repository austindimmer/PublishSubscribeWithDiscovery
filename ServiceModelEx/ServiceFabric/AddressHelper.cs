// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx.ServiceFabric
{
   public static class AddressHelper
   {
      public static bool EvaluateAddress(Uri serviceAddress,out string applicationName,out string serviceName)
      {
         string[] parts = serviceAddress.ToString().Split('/');
         if(!parts[0].Equals("fabric:") || parts.Length != 3)
         {
            throw new ArgumentException("Invalid name Uri.");
         }
         applicationName = parts[1];
         serviceName = parts[2];
         return true;
      }

      public static class Wcf
      {
         public static Uri BuildAddress(string baseAddress,string applicationName,string serviceName,Type interfaceType)
         {
            string[] addressParts = baseAddress.Split(':');
            string host = addressParts[0];
            int port = baseAddress.Split(':').Length == 2 ? Convert.ToInt32(addressParts[1]) : 808;
            Uri uri = new UriBuilder("net.tcp",baseAddress,port,applicationName + "/" + serviceName + "/" + interfaceType.Name).Uri;
            return uri;
         }
         public static Uri BuildAddress<I>(string baseAddress,string applicationName,string serviceName) where I : class
         {
            return BuildAddress(baseAddress,applicationName,serviceName,typeof(I));
         }
      }
   }
}
