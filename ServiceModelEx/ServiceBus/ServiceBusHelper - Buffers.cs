// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using Microsoft.ServiceBus;


namespace ServiceModelEx.ServiceBus
{
   static partial class ServiceBusHelper
   {
      internal static void VerifyOneway(Type interfaceType)
      {
         Debug.Assert(interfaceType.IsInterface);

         MethodInfo[] methods = interfaceType.GetMethods();
         foreach(MethodInfo method in methods)
         {
            object[] attributes = method.GetCustomAttributes(typeof(OperationContractAttribute),true);
            Debug.Assert(attributes.Length == 1);

            OperationContractAttribute attribute = attributes[0] as OperationContractAttribute;
            if(attribute.IsOneWay == false)
            {
               throw new InvalidOperationException("All operations on contract " + interfaceType + " must be one-way, but operation " + method.Name + " is not configured for one-way");
            }
         }
      }
   }
}






