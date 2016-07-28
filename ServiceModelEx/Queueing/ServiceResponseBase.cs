// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;

namespace ServiceModelEx
{
   public abstract class ServiceResponseBase<T> : HeaderClientBase<T,ResponseContext> where T : class
   {
      public ServiceResponseBase() : this(OperationContext.Current.Host.Description.Endpoints[0].Binding as NetMsmqBinding)
      {}
      public ServiceResponseBase(NetMsmqBinding binding) : base(ResponseContext.Current,
                                                                binding,
                                                                new EndpointAddress(ResponseContext.Current.ResponseAddress))
      {
         Endpoint.VerifyQueue();
      }
      public ServiceResponseBase(string bindingName) : this(new NetMsmqBinding(bindingName))
      {}
   }
}
