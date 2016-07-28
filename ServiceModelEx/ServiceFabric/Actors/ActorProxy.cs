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
using ServiceModelEx.ServiceFabric.Test;

namespace ServiceModelEx.ServiceFabric.Actors
{
   public static class ActorProxy
   {
      static readonly MethodInfo m_InitializeInstanceDefinition = null;
      static readonly NetNamedPipeContextBinding m_ActorBinding = null;
      static readonly NetNamedPipeContextBinding m_ProxyBinding = null;

      internal static NetNamedPipeContextBinding ActorBinding
      {
         get
         {
            return m_ActorBinding;
         }
      }
      internal static NetNamedPipeContextBinding ProxyBinding
      {
         get
         {
            return m_ProxyBinding;
         }
      }

      static ActorProxy()
      {
         m_InitializeInstanceDefinition = typeof(InProcFactory).GetMethod("InitializeInstance",
                                                                          BindingFlags.NonPublic|BindingFlags.Static,
                                                                          null,
                                                                          new Type[] {typeof(IServiceBehavior),typeof(IEndpointBehavior),typeof(NetNamedPipeContextBinding),typeof(NetNamedPipeContextBinding)},
                                                                          null).GetGenericMethodDefinition();

         m_ProxyBinding = BindingHelper.Actor.ProxyBinding("ActorProxyBinding");
         m_ActorBinding = BindingHelper.Actor.Binding();
      }

      static bool IsInfrastructureEndpoint(Type interfaceType)
      {
         return interfaceType.Equals(typeof(IActor)) ||
                interfaceType.Equals(typeof(IStatefulActorManagement));
      }
      public static I Create<I>(ActorId actorId,Uri serviceAddress) where I : class,IActor
      {
         string applicationName = string.Empty, 
                serviceName = string.Empty;

         AddressHelper.EvaluateAddress(serviceAddress,out applicationName,out serviceName);
         return Create<I>(actorId,applicationName,serviceName);
      }
      public static I Create<I>(ActorId actorId,string applicationName = null,string serviceName = null) where I : class,IActor
      {
         Debug.Assert(FabricRuntime.Actors.ContainsKey(applicationName));

         Type actorType = FabricRuntime.Actors[applicationName].SingleOrDefault(type=>type.GetInterfaces().Where(interfaceType=>((IsInfrastructureEndpoint(interfaceType) == false) && (interfaceType == typeof(I)))).Any());
         Debug.Assert(actorType != null);
         Debug.Assert(actorType.BaseType != null);

         bool actorExists = actorType.GetCustomAttributes<ApplicationManifestAttribute>().Where(attribute=>attribute.ApplicationName.Equals(applicationName)).Any(attribute=>attribute.ServiceName.Equals(serviceName));
         Debug.Assert(actorExists);
         if(!actorExists)
         {
            throw new InvalidOperationException("Service does not exist.");
         }

         IServiceBehavior actorBehavior = null;
         if(actorType.BaseType.Equals(typeof(StatelessActor)))
         {
            actorBehavior = new StatelessActorBehavior();
         }
         else if(actorType.BaseType.IsGenericType)
         {
            Type genericTypeDef = actorType.BaseType.GetGenericTypeDefinition();
            if(genericTypeDef.Equals(typeof(StatefulActor<>)))
            {
               actorBehavior = new StatefulActorBehavior();
            }
         }

         if(actorBehavior == null)
         {
            throw new InvalidOperationException("Validation failed.");
         }

         if((ServiceTestBase.ActorMocks != null) && (ServiceTestBase.ActorMocks.ContainsKey(actorType)) && (ServiceTestBase.ActorMocks[actorType] != null))
         {
            if(ServiceTestBase.ActorMocks[actorType] is Moq.Mock)
            {
               return ((Moq.Mock)ServiceTestBase.ActorMocks[actorType]).Object as I;
            }
            else
            {
               return ServiceTestBase.ActorMocks[actorType] as I;
            }
         }

         try
         {
            actorId.ApplicationName = applicationName;
            actorId.ActorInterfaceName = typeof(I).FullName;

            MethodInfo initializeInstance = m_InitializeInstanceDefinition.MakeGenericMethod(actorType,typeof(I));
            ChannelFactory<I> factory = initializeInstance.Invoke(null,new object[] {actorBehavior,new ProxyMessageInterceptor(actorId),m_ProxyBinding,m_ActorBinding}) as ChannelFactory<I>;
            I channel = new ActorChannelInvoker<I>().Install(factory,actorId);
            channel.ActivateAsync().Wait();
            return channel;
         }
         catch (TargetInvocationException exception)
         {
            throw exception.InnerException;
         }
      }
   }
}
