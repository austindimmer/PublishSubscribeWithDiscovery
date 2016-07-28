// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace ServiceModelEx
{
   public class DuplexChannelFactory<T,C> : DuplexChannelFactory<T> where T : class
   {
      static DuplexChannelFactory()
      {
         DuplexClientBase<T,C>.VerifyCallback();
      }
      public DuplexChannelFactory(C callback) : base(callback)
      {}
      public DuplexChannelFactory(InstanceContext<C> context,Binding binding) : base(context.Context,binding)
      {}
      public DuplexChannelFactory(InstanceContext<C> context,ServiceEndpoint endpoint) : base(context.Context,endpoint)
      {}

      public DuplexChannelFactory(InstanceContext<C> context,string endpointName) : base(context.Context,endpointName)
      {}
      public DuplexChannelFactory(C callback,Binding binding) : base(callback,binding)
      {}
      public DuplexChannelFactory(C callback,ServiceEndpoint endpoint): base(callback,endpoint)
      {}
      public DuplexChannelFactory(C callback,string endpointName): base(callback,endpointName)
      {}
      public DuplexChannelFactory(InstanceContext<C> context,Binding binding,EndpointAddress endpointAddress): base(context.Context,binding,endpointAddress)
      {}
      public DuplexChannelFactory(InstanceContext<C> context,string endpointName,EndpointAddress endpointAddress): base(context.Context,endpointName,endpointAddress)
      {}
      public DuplexChannelFactory(C callback,Binding binding,EndpointAddress endpointAddress): base(callback,binding,endpointAddress)
      {}
      public DuplexChannelFactory(C callback,string endpointName,EndpointAddress endpointAddress): base(callback,endpointName,endpointAddress)
      {}
      public static T CreateChannel(C callback,string endpointName)
      {
         return DuplexChannelFactory<T>.CreateChannel(callback,endpointName);
      }
      public static T CreateChannel(InstanceContext<C> context,string endpointName)
      {
         return DuplexChannelFactory<T>.CreateChannel(context.Context,endpointName);
      }
      public static T CreateChannel(C callback,Binding binding,EndpointAddress endpointAddress)
      {
         return DuplexChannelFactory<T>.CreateChannel(callback,binding,endpointAddress);
      }
      public static T CreateChannel(InstanceContext<C> context,Binding binding,EndpointAddress endpointAddress)
      {
         return DuplexChannelFactory<T>.CreateChannel(context.Context,binding,endpointAddress);
      }
   }
}
