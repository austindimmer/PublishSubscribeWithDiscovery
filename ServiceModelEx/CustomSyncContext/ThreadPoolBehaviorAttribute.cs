// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class ThreadPoolBehaviorAttribute : Attribute,IContractBehavior,IServiceBehavior
   {
      protected string PoolName
      {
         get;set;
      }
      protected uint PoolSize
      {
         get;set;
      }
      protected Type ServiceType
      {
         get;set;
      }

      public ThreadPoolBehaviorAttribute(uint poolSize,Type serviceType) : this(poolSize,serviceType,null)
      {}
      public ThreadPoolBehaviorAttribute(uint poolSize,Type serviceType,string poolName)
      {
         PoolName = poolName;
         ServiceType = serviceType;
         PoolSize = poolSize;
      }
      protected virtual ThreadPoolSynchronizer ProvideSynchronizer()
      {
         if(ThreadPoolHelper.HasSynchronizer(ServiceType) == false)
         {
            return new ThreadPoolSynchronizer(PoolSize,PoolName);
         }
         else
         {
            return ThreadPoolHelper.GetSynchronizer(ServiceType);
         }
      }
      void IContractBehavior.AddBindingParameters(ContractDescription description,ServiceEndpoint endpoint,BindingParameterCollection parameters)
      {}

      void IContractBehavior.ApplyClientBehavior(ContractDescription description,ServiceEndpoint endpoint,ClientRuntime proxy)
      {}

      void IContractBehavior.ApplyDispatchBehavior(ContractDescription description,ServiceEndpoint endpoint,DispatchRuntime dispatchRuntime)
      {
         PoolName = PoolName ?? "Pool executing endpoints of " + ServiceType;
         lock(typeof(ThreadPoolHelper))
         {
            ThreadPoolHelper.ApplyDispatchBehavior(ProvideSynchronizer(),PoolSize,ServiceType,PoolName,dispatchRuntime);
         }
      }
      void IContractBehavior.Validate(ContractDescription description,ServiceEndpoint endpoint)
      {}

      void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
      {}

      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
      {}

      void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase serviceHostBase)
      {
         serviceHostBase.Closed += delegate
                                   {
                                      ThreadPoolHelper.CloseThreads(ServiceType);
                                   };
      }
   }
}