// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;

namespace ServiceModelEx
{
   public static class DebugHelper
   {
      public const bool IncludeExceptionDetailInFaults = 
      #if DEBUG
            true;
      #else
            false;
      #endif

      public static Exception ExtractException(this FaultException<ExceptionDetail> fault)
      {
         return ExtractException(fault.Detail);
      }
      static Exception ExtractException(ExceptionDetail detail)
      {
         Exception innerException = null;
         if(detail.InnerException != null)
         {
            innerException = ExtractException(detail.InnerException);
         }
         Type type = Type.GetType(detail.Type);
         Debug.Assert(type != null,"Make sure this assembly (ServiceModelEx by default) contains the definition of the custom exception");
         Debug.Assert(type.IsSubclassOf(typeof(Exception)));

         Type[] parameters = {typeof(string),typeof(Exception)};
         ConstructorInfo info = type.GetConstructor(parameters);
         Debug.Assert(info != null,"Exception type " + detail.Type + " does not have suitable constructor");

         Exception exception = Activator.CreateInstance(type,detail.Message,innerException) as Exception;
         Debug.Assert(exception != null);
         return exception;
      }
   }
}