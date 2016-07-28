// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;

using ServiceModelEx.Fabric;
using ServiceModelEx.ServiceFabric.Services.Runtime;

namespace ServiceModelEx.ServiceFabric.Services.Remoting.Client
{
   public static class ServiceProxy
   {
      static readonly MethodInfo m_InitializeInstanceDefinition = null;
      static readonly NetNamedPipeContextBinding m_ServiceBinding = null;
      static readonly NetNamedPipeContextBinding m_ProxyBinding = null;

      internal static NetNamedPipeContextBinding ServiceBinding
      {
         get
         {
            return m_ServiceBinding;
         }
      }
      internal static NetNamedPipeContextBinding ProxyBinding
      {
         get
         {
            return m_ProxyBinding;
         }
      }

      static ServiceProxy()
      {
         m_InitializeInstanceDefinition = typeof(InProcFactory).GetMethod("InitializeInstance",
                                                                          BindingFlags.NonPublic|BindingFlags.Static,
                                                                          null,
                                                                          new Type[] {typeof(IServiceBehavior),typeof(IEndpointBehavior),typeof(NetNamedPipeContextBinding),typeof(NetNamedPipeContextBinding)},
                                                                          null).GetGenericMethodDefinition();

         m_ProxyBinding = BindingHelper.Service.Default.ProxyBinding("ServiceProxyBinding");
         m_ServiceBinding = BindingHelper.Service.Default.Binding();
      }

      public static I Create<I>(Uri serviceAddress,string listenerName = null) where I : class
      {
         string applicationName = string.Empty, 
                serviceName = string.Empty;

         Debug.Assert(listenerName.Equals(typeof(I).Name));

         AddressHelper.EvaluateAddress(serviceAddress,out applicationName,out serviceName);
         return Create<I>(applicationName,serviceName);
      }
      static I Create<I>(string applicationName,string serviceName) where I : class
      {
         Debug.Assert(FabricRuntime.Services != null,"Use ServiceProxy only within Service Fabric");
         Debug.Assert(FabricRuntime.Services.ContainsKey(applicationName));

         Type serviceType = FabricRuntime.Services[applicationName].SingleOrDefault(type=>type.GetInterfaces().Where(interfaceType=>interfaceType == typeof(I)).Any());
         Debug.Assert(serviceType != null);
         Debug.Assert(serviceType.BaseType != null);

         bool serviceExists = serviceType.GetCustomAttributes<ApplicationManifestAttribute>().Where(attribute=>attribute.ApplicationName.Equals(applicationName)).Any(attribute=>attribute.ServiceName.Equals(serviceName));
         Debug.Assert(serviceExists);
         if(!serviceExists)
         {
            throw new InvalidOperationException("Service does not exist.");
         }

         IServiceBehavior serviceBehavior = null;
         if(serviceType.BaseType.Equals(typeof(StatelessService)))
         {
            serviceBehavior = new StatelessServiceBehavior();
         }
         else
         {
            throw new InvalidOperationException("Validation failed.");
         }

         try
         {
            MethodInfo initializeInstance = m_InitializeInstanceDefinition.MakeGenericMethod(serviceType,typeof(I));
            ChannelFactory<I> factory = initializeInstance.Invoke(null,new object[] {serviceBehavior,new ProxyMessageInterceptor(),m_ProxyBinding,m_ServiceBinding}) as ChannelFactory<I>;
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
