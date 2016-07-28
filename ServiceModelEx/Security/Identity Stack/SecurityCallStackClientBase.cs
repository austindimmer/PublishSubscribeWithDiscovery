// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.ServiceModel.Channels;


namespace ServiceModelEx
{
   public abstract class SecurityCallStackClientBase<T> : HeaderClientBase<T,SecurityCallStack> where T : class
   {
      protected SecurityCallStackClientBase()
      {
         InitializeCallStack();
      }
    
      public SecurityCallStackClientBase(string endpointConfigurationName) : base(endpointConfigurationName)
      {
         InitializeCallStack();
      }

      public SecurityCallStackClientBase(string endpointConfigurationName,string remoteAddress) : base(endpointConfigurationName,remoteAddress)
      {
         InitializeCallStack();
      }

      public SecurityCallStackClientBase(string endpointConfigurationName,EndpointAddress remoteAddress) : base(endpointConfigurationName,remoteAddress)
      {
         InitializeCallStack();
      }

      public SecurityCallStackClientBase(Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         InitializeCallStack();
      }

      void InitializeCallStack()
      {
         if(OperationContext.Current != null)
         {
            Header = SecurityCallStackContext.Current;

            if(Header == null)
            {
               Header = new SecurityCallStack();
            }
         }
         else
         {
            Header = new SecurityCallStack();
         }
      }
      protected override void PreInvoke(ref Message reply)
      {
         Header.AppendCall();
         base.PreInvoke(ref reply);
      }  
   }
}