// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Runtime.Serialization;

namespace ServiceModelEx
{
   [DataContract]
   public class ResponseContext
   {
      [DataMember]
      public readonly string ResponseAddress;

      [DataMember]
      public readonly string FaultAddress;

      [DataMember]
      public readonly string MethodId;

      public ResponseContext(string responseAddress,string methodId) : this(responseAddress,methodId,null)
      {}
      public ResponseContext(ResponseContext responseContext) : this(responseContext.ResponseAddress,responseContext.MethodId,responseContext.FaultAddress)
      {}
      public ResponseContext(string responseAddress) : this(responseAddress,Guid.NewGuid().ToString())
      {}
      public ResponseContext(string responseAddress,string methodId,string faultAddress)
      {
         ResponseAddress = responseAddress;
         MethodId = methodId;
         FaultAddress = faultAddress;
      }
      public static ResponseContext Current
      {
         get
         {
            return GenericContext<ResponseContext>.Current.Value;
         }
         set
         {
            GenericContext<ResponseContext>.Current = new  GenericContext<ResponseContext>(value);
         }
      }
   }
}





