// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceModelEx
{
   public abstract class DuplexClientBase<T,C> : DuplexClientBase<T> where T : class
   {
      static DuplexClientBase()
      {
         VerifyCallback();
      }
      internal static void VerifyCallback()
      {
         Type contractType = typeof(T);
         Type callbackType = typeof(C);
         object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute),false);
         if(attributes.Length == 0)
         {
            throw new InvalidOperationException("Type of " + contractType + " is not a service contract");
         }
         ServiceContractAttribute serviceContractAttribute = attributes[0] as ServiceContractAttribute;
         if(callbackType != serviceContractAttribute.CallbackContract)
         {
            throw new InvalidOperationException("Type of " + callbackType + " is not configured as callback contract for " + contractType);
         }
      }
      protected DuplexClientBase(InstanceContext<C> context) : base(context.Context)
      {}
      protected DuplexClientBase(InstanceContext<C> context,string endpointName) : base(context.Context,endpointName)
      {}
      protected DuplexClientBase(InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context.Context,binding,remoteAddress)
      {}
      protected DuplexClientBase(InstanceContext<C> context,string endpointName,EndpointAddress remoteAddress) : base(context.Context,endpointName,remoteAddress)
      {}
      protected DuplexClientBase(InstanceContext<C> context,string endpointName,string remoteAddress) : base(context.Context,endpointName,remoteAddress)
      {}

      protected DuplexClientBase(C callback) : base(callback)
      {}
      protected DuplexClientBase(C callback,string endpointName) : base(callback,endpointName)
      {}
      protected DuplexClientBase(C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {}
      protected DuplexClientBase(C callback,string endpointName,EndpointAddress remoteAddress) : base(callback,endpointName,remoteAddress)
      {}
      protected DuplexClientBase(C calback,string endpointName,string remoteAddress) : base(calback,endpointName,remoteAddress)
      {}
   }
}
