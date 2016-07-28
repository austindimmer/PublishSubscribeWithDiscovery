// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Runtime.Serialization;

namespace ServiceModelEx
{
   [DataContract]
   public class SecurityCallFrame
   {
      internal SecurityCallFrame()
      {
         CallTime = DateTime.UtcNow;
      }
      internal SecurityCallFrame(string address,string operation,string service,string authentication,string identityName,Guid activityId)
      {
         Address = address;
         Operation = operation;
         CallTime = DateTime.UtcNow;
         Authentication = authentication;
         IdentityName = identityName;
         ActivityId = activityId;
         CallerType = String.Empty;
      }

      [DataMember(IsRequired = true)]
      public Guid ActivityId
      {
         get;set;
      }

      [DataMember(IsRequired = true)]
      public string Authentication
      {
         get;internal set;
      }

      [DataMember(IsRequired = true)]
      public string CallerType
      {
         get;internal set;
      }
      [DataMember(IsRequired = true)]
      public string IdentityName
      {
         get;internal set;
      }
      [DataMember(IsRequired = true)]
      public string Address
      {
         get;set;
      }

      [DataMember(IsRequired = true)]
      public string Operation
      {
         get;set;
      }

      [DataMember(IsRequired = true)]
      public DateTime CallTime
      {
         get;private set;
      }
   }
}
