// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Reflection;
using System.ServiceModel;

using ServiceModelEx.ServiceFabric.Services.Communication.Client;

namespace ServiceModelEx.ServiceFabric.Services.Communication.Wcf.Client
{
   public class WcfCommunicationClient<I> : ICommunicationClient where I : class
   {
      public I Channel
      {
         get
         {
            return CreateChannel();
         }
      }
      internal string ApplicationName
      {get; private set;}
      internal string ServiceName
      {get; private set;}
      internal EndpointAddress Address
      {get; private set;}
      internal NetTcpBinding Binding
      {get; private set;}

      internal WcfCommunicationClient(string baseAddress,string applicationName,string serviceName,NetTcpBinding binding)
      {
         ApplicationName = applicationName;
         ServiceName = serviceName;
         Address = new EndpointAddress(AddressHelper.Wcf.BuildAddress<I>(baseAddress,applicationName,serviceName));
         Binding = binding;
      }

      I CreateChannel()
      {
         try
         {
            ChannelFactory<I> factory = new ChannelFactory<I>(Binding,Address);
            I channel = new ServiceChannelInvoker<I>().Install(factory);
            return channel;
         }
         catch (TargetInvocationException exception)
         {
            throw exception.InnerException;
         }
      }
   }
}
