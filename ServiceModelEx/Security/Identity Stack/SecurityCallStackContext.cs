// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Runtime.Serialization;

namespace ServiceModelEx
{
   [DataContract]
   public class SecurityCallStackContext
   {
      public static SecurityCallStack Current
      {
         get
         {
            if(GenericContext<SecurityCallStack>.Current == null)
            {
               return null;
            }
            return GenericContext<SecurityCallStack>.Current.Value;
         }
         set
         {
            GenericContext<SecurityCallStack>.Current = new GenericContext<SecurityCallStack>(value);
         }
      }
   }
}