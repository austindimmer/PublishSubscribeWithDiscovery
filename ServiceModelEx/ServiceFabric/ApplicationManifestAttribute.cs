// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx.ServiceFabric
{
   [AttributeUsage(AttributeTargets.Class,AllowMultiple=true)]
   public class ApplicationManifestAttribute : Attribute
   {
      public string ApplicationName
      {get;private set;}
      public string ServiceName
      {get;private set;}

      public ApplicationManifestAttribute(string ApplicationName,string ServiceName)
      {
         this.ApplicationName = ApplicationName;
         this.ServiceName = ServiceName;
      }
   }
}
