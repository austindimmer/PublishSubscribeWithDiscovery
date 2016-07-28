// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceModelEx
{
   public class HeaderChannelFactory<T,H> : InterceptorChannelFactory<T> where T : class
   {
      public H Header
      {get;protected set;}

      public HeaderChannelFactory() : this(default(H))
      {}

      public HeaderChannelFactory(string endpointName) : this(default(H),endpointName)
      {}

      public HeaderChannelFactory(Binding binding,EndpointAddress remoteAddress) : this(default(H),binding,remoteAddress)
      {}

      public HeaderChannelFactory(H header)
      {
         Header = header;
      }

      public HeaderChannelFactory(H header,string endpointName) : base(endpointName)
      {
         Header = header;
      }

      public HeaderChannelFactory(H header,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         Header = header;
      }
      protected override void PreInvoke(ref Message request)
      {
         GenericContext<H> context = new GenericContext<H>(Header);
         MessageHeader<GenericContext<H>> genericHeader = new MessageHeader<GenericContext<H>>(context);
         request.Headers.Add(genericHeader.GetUntypedHeader(GenericContext<H>.TypeName,GenericContext<H>.TypeNamespace));
      }
   }
}