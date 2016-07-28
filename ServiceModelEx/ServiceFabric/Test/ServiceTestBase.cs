// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Description;

using Moq;
using ServiceModelEx;
using ServiceModelEx.Fabric;
using ServiceModelEx.ServiceFabric.Actors;

namespace ServiceModelEx.ServiceFabric.Test
{
   public abstract class ServiceTestBase
   {
      static MethodInfo m_CreateInstanceDefinition = null;
      static ServiceTestBase()
      {
         m_CreateInstanceDefinition = typeof(InProcFactory).GetMethod("CreateInstance",
                                                                      BindingFlags.NonPublic|BindingFlags.Static,
                                                                      null,
                                                                      new Type[] {typeof(IServiceBehavior)},
                                                                      null).GetGenericMethodDefinition();
      }

      readonly Type[] m_ServicesUnderTest;
      readonly Type[] m_ActorsUnderTest;

      public static Dictionary<Type,object> ServiceMocks
      {get; private set;}
      public static Dictionary<Type,object> ActorMocks
      {get; private set;}

      protected ServiceTestBase()
      {
         m_ServicesUnderTest = FabricRuntime.Services.Values.SelectMany(services=>services).Distinct().ToArray();
         ServiceMocks = m_ServicesUnderTest.ToDictionary(key=>key,key=>(object)null);
         m_ActorsUnderTest = FabricRuntime.Actors.Values.SelectMany(actors=>actors).Distinct().ToArray();
         ActorMocks = m_ActorsUnderTest.ToDictionary(key=>key,key=>(object)null);
      }

      Type GetServiceType<I>() where I : class
      {
         Type serviceType = m_ServicesUnderTest.SingleOrDefault(service=>service.GetInterfaces().Where(contract=>contract == typeof(I)).Any());
         return serviceType;
      }
      Type GetActorType<I>() where I : class,IActor
      {
         Type actorType = m_ActorsUnderTest.SingleOrDefault(actor=>actor.GetInterfaces().Where(contract=>contract == typeof(I)).Any());
         return actorType;
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      protected void RegisterMocks(params object[] mocks)
      {
         foreach(object mock in mocks)
         {
            Func<Type,bool> hasContract = null;
            if(mock is Mock)
            {
               hasContract = (contract)=>contract == mock.GetType().GetGenericArguments()[0];
            }
            else
            {
               hasContract = (contract)=>mock.GetType().GetInterfaces().Any(mockContract=>mockContract.Equals(contract));
            }
            Type actorType = ActorMocks.Keys.FirstOrDefault(actor=>actor.GetInterfaces().Any(hasContract));
            ActorMocks[actorType] = mock;
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      protected void UnregisterMocks()
      {
         Type[] actors = ActorMocks.Keys.ToArray();
         if(actors != null)
         {
            ActorMocks = actors.ToDictionary(k=>k,k=>(object)null);
         }
      }

      void MockEnvironment<I,S>(I callee,Action<I> callerMock,S state,params object[] mocks) where I : class
                                                                                             where S : class,new()
      {
         try
         {
            IClientChannel channel = callee as IClientChannel;
            if((state != null) && (channel == null) && (callee is IActor))
            {
               StatefulActor<S> actor = callee as StatefulActor<S>;
               actor.GetType().InvokeMember("State",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.SetProperty,null,actor,new object[] {state});
            }

            RegisterMocks(mocks);
            using (callee as IDisposable)
            {
               callerMock(callee);
            }
            UnregisterMocks();
         }
         finally
         {
            IClientChannel channel = callee as IClientChannel;
            if(channel != null)
            {
               if(channel.State != CommunicationState.Closed && channel.State != CommunicationState.Faulted)
               {
                  try
                  {
                     channel.Close();
                  }
                  catch
                  {
                     channel.Abort();
                  }
               }
            }
         }
      }
      void MockPocoEnvironment<I,S>(Type targetType,Action<I> callerMock,S state,params object[] mocks) where I : class
                                                                                                        where S : class,new() 
      {
         I poco =  Activator.CreateInstance(targetType) as I;
         MockEnvironment<I,S>(poco,callerMock,state,mocks);
      }
      void MockServiceEnvironment<I,S>(Type targetType,Action<I> callerMock,S state,params object[] mocks) where I : class
                                                                                                           where S : class,new() 
      {
         MethodInfo createInstance = m_CreateInstanceDefinition.MakeGenericMethod(targetType,typeof(I));
         I proxy = (I)createInstance.Invoke(null,new object[] {new TestActorServiceBehavior<S>(state)});
         MockEnvironment<I,S>(proxy,callerMock,state,mocks);
      }

      public void TestActorPoco<I>(Action<I> callerMock,params object[] mocks) where I : class,IActor
      {
         TestActorPoco<I,object>(callerMock,null,mocks);
      }
      public void TestActorPoco<I,S>(Action<I> callerMock,S state,params object[] mocks) where I : class,IActor
                                                                                         where S : class,new()
      {
         MockPocoEnvironment<I,S>(GetActorType<I>(),callerMock,state,mocks);
      }
      public void TestActor<I>(Action<I> callerMock,params object[] mocks) where I : class,IActor
      {
         TestActor<I,object>(callerMock,null,mocks);
      }
      public void TestActor<I,S>(Action<I> callerMock,S state,params object[] mocks) where I : class,IActor 
                                                                                     where S : class,new()
      {
         MockServiceEnvironment<I,S>(GetActorType<I>(),callerMock,state,mocks);
      }

      public void TestServicePoco<I>(Action<I> callerMock,params object[] mocks) where I : class
      {
         MockPocoEnvironment<I,object>(GetServiceType<I>(),callerMock,null,mocks);
      }
      public void TestService<I>(Action<I> callerMock,params object[] mocks) where I : class
      {
         MockServiceEnvironment<I,object>(GetServiceType<I>(),callerMock,null,mocks);
      }
   }
}
