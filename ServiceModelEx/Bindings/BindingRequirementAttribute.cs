// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ServiceBus;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class BindingRequirementAttribute : Attribute,IServiceBehavior,IEndpointBehavior
   {
      public bool ReliabilityRequired
      {
         get;set;
      }

      public bool WCFOnly
      {
         get;set;
      }

      public bool TransactionFlowEnabled
      {
         get;set;
      }

      static void ValidateWCF(ServiceEndpoint endpoint)
      {
         if(endpoint.Binding is NetTcpBinding       || 
            endpoint.Binding is NetNamedPipeBinding ||
            endpoint.Binding is NetMsmqBinding      ||
            endpoint.Binding is NetTcpRelayBinding  ||
            endpoint.Binding is NetOnewayRelayBinding)
         {
            return;
         }
         throw new InvalidOperationException("BindingRequirementAttribute requires WCF-to-WCF binding only, but binding for the endpoint with contract " + endpoint.Contract.ContractType + " is not.");
      }
      static void ValidateReliability(ServiceEndpoint endpoint)
      {
         if(endpoint.Binding is NetNamedPipeBinding)//Inherently reliable
         {
            return;
         }
         if(endpoint.Binding is WSDualHttpBinding)//Always reliable
         {
            return;
         }             
         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding tcpBinding = endpoint.Binding as NetTcpBinding;
            if(tcpBinding.ReliableSession.Enabled)
            {
               return;
            }
         }
         if(endpoint.Binding is WSHttpBindingBase)
         {
            WSHttpBindingBase wsBinding = endpoint.Binding as WSHttpBindingBase;
            if(wsBinding.ReliableSession.Enabled)
            {
               return;
            }
         }
         throw new InvalidOperationException("BindingRequirementAttribute requires reliability enabled, but binding for the endpoint with contract " + endpoint.Contract.ContractType + " has does not support reliability or has it disabled");
      }
      static void ValidateTransactionFlow(ServiceEndpoint endpoint)
      {
         Exception exception = new InvalidOperationException("BindingRequirementAttribute requires transaction flow enabled, but binding for the endpoint with contract " + endpoint.Contract.ContractType + " has it disabled");

         foreach(OperationDescription operation in endpoint.Contract.Operations)
         {
            TransactionFlowAttribute attribute = operation.Behaviors.Find<TransactionFlowAttribute>();
            if(attribute != null)
            {
               if(attribute.Transactions == TransactionFlowOption.Allowed)
               {
                  try
                  {
                     dynamic binding = endpoint.Binding;
                     if(binding.TransactionFlow == false)
                     {
                        throw exception;
                     }
                     continue;
                  }
                  catch(RuntimeBinderException)
                  {
                     throw new InvalidOperationException("BindingRequirementAttribute requires transaction flow enabled, but binding for the endpoint with contract " + endpoint.Contract.ContractType + " does not support transaction flow");
                  }
               }
            }
         }
      }
      void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase host) 
      {
         IEndpointBehavior endpointBehavior = this;
         foreach(ServiceEndpoint endpoint in description.Endpoints)
         {
            endpointBehavior.Validate(endpoint);
         }
      }
      void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase host,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
      {}
      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase host)
      {}

      void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint,BindingParameterCollection bindingParameters)
      {}

      void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint,ClientRuntime behavior)
      {}

      void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint,EndpointDispatcher endpointDispatcher)
      {}

      void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
      {
         if(WCFOnly)
         {
            ValidateWCF(endpoint);
         }
         if(TransactionFlowEnabled)
         {
            ValidateTransactionFlow(endpoint);
         }
         if(ReliabilityRequired)
         {
            ValidateReliability(endpoint);
         }
      }
   }
} 





