// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [Serializable]
   internal class ActorInfo
   {
      public ActorId ActorId
      {get;set;}
      public Guid DurableInstanceId
      {get;set;}
      public bool StatefulActor
      {get;set;}
      public Type ActorImplementationType
      {get;set;}
      public Type ActorInterfaceType
      {get;set;}
      public DateTime IdleStartTime
      {get;set;}
      public ActorGarbageCollectionAttribute GarbageColllectionSettings
      {get;set;}
   }

   [Serializable]
   class ActorIdComparer : IEqualityComparer<ActorId>
   {
      public bool Equals(ActorId x,ActorId y)
      {
         return x.ApplicationName.Equals(y.ApplicationName) && x.ActorInterfaceName.Equals(y.ActorInterfaceName) && x.ToString().Equals(y.ToString());
      }
      public int GetHashCode(ActorId obj)
      {
         return (obj.ApplicationName + obj.ActorInterfaceName + obj.ToString()).GetHashCode();
      }
   }

   internal static class ActorManager
   {
      static ConcurrentDictionary<ActorId,ActorInfo> m_VolatileInstances = new ConcurrentDictionary<ActorId,ActorInfo>(new ActorIdComparer());
      static ConcurrentDictionary<ActorId,ActorInfo> m_PersistentInstances = new ConcurrentDictionary<ActorId,ActorInfo>(new ActorIdComparer());
      const string m_Filename = "PersistentActorIds.bin";
      static object m_FileLock = new object();
      static object m_ProcessActorLock = new object();
      static Timer m_DeactivationTimer = null;
      static readonly MethodInfo m_InitializeInstanceDefinition = null;

      static IEnumerable<Task> CheckActors(bool CompleteActor,
                                           ConcurrentDictionary<ActorId,ActorInfo> instances,
                                           Action<KeyValuePair<ActorId,ActorInfo>,ConcurrentDictionary<ActorId,ActorInfo>> action)
      {
         Action<KeyValuePair<ActorId,ActorInfo>> checkActor = (pair)=>
                                                              {
                                                                 long IdleTimeoutInSeconds = -1;
                                                                 if(pair.Value.GarbageColllectionSettings != null)
                                                                 {
                                                                    IdleTimeoutInSeconds = pair.Value.GarbageColllectionSettings.IdleTimeoutInSeconds;
                                                                 }
                                                                 if(IdleTimeoutInSeconds != -1)
                                                                 {
                                                                    try
                                                                    {
                                                                       if((DateTime.Now - pair.Value.IdleStartTime).TotalSeconds > IdleTimeoutInSeconds)
                                                                       {
                                                                          action(pair,instances);
                                                                          pair.Value.IdleStartTime = DateTime.MaxValue;
                                                                       }
                                                                    }
                                                                    catch(Exception exception)
                                                                    {
                                                                       Console.WriteLine("CheckActors exception :" + exception.Message);
                                                                    }
                                                                 }
                                                              };

         return instances.ForEachAsync(checkActor);
      }
      static void Deactivate(KeyValuePair<ActorId,ActorInfo> pair,ConcurrentDictionary<ActorId,ActorInfo> instances)
      {
         MethodInfo initializeInstance = m_InitializeInstanceDefinition.MakeGenericMethod(pair.Value.ActorImplementationType,typeof(IActor));
         ChannelFactory<IActor> factory = initializeInstance.Invoke(null,new object[] {null,new ProxyMessageInterceptor(pair.Value.ActorId),ActorProxy.ProxyBinding,ActorProxy.ActorBinding}) as ChannelFactory<IActor>;
         IActor proxy = new ActorChannelInvoker<IActor>().Install(factory,pair.Value.ActorId);
         proxy.DeactivateAsync().Wait();
         factory.Close();
      }
      static void ProcessActors(bool CompleteActor,object state,Action<KeyValuePair<ActorId,ActorInfo>,ConcurrentDictionary<ActorId,ActorInfo>> action)
      {
         lock(m_ProcessActorLock)
         {
            List<Task> tasks = new List<Task>();
            tasks.AddRange(CheckActors(CompleteActor,m_VolatileInstances,action));
            tasks.AddRange(CheckActors(CompleteActor,m_PersistentInstances,action));
            Task.WhenAll(tasks).Wait();
            SaveFile(m_PersistentInstances);
         }
      }
      static void DeactivateActors(object state)
      {
         ProcessActors(false,state,Deactivate);
      }

      static void SaveFile(ConcurrentDictionary<ActorId,ActorInfo> instances)
      {
         lock(m_FileLock)
         {
            try
            {
               using(Stream stream = new FileStream(m_Filename,FileMode.OpenOrCreate,FileAccess.Write))
               {
                  IFormatter formatter = new BinaryFormatter();
                  formatter.Serialize(stream,instances);
               }
            }
            catch
            {
               throw new InvalidOperationException("ActorId management failed. Could not write to the persistent ActorId file: " + m_Filename);
            }
         }
      }
      static void SaveInstance(ActorId actorId,ActorInfo state,bool persist,ConcurrentDictionary<ActorId,ActorInfo> instances)
      {
         if(!instances.ContainsKey(actorId))
         {
            if(!instances.TryAdd(actorId,state))
            {
               Debug.Assert(false);
            }
            if(persist)
            {
               SaveFile(instances);
            }
         }
      }

      static ActorManager()
      {
         m_InitializeInstanceDefinition = typeof(InProcFactory).GetMethod("InitializeInstance",
                                                                          BindingFlags.NonPublic|BindingFlags.Static,
                                                                          null,
                                                                          new Type[] {typeof(IServiceBehavior),typeof(IEndpointBehavior),typeof(NetNamedPipeContextBinding),typeof(NetNamedPipeContextBinding)},
                                                                          null).GetGenericMethodDefinition();

         m_DeactivationTimer = new Timer(DeactivateActors,null,1000,1000);

         using(Stream stream = new FileStream(m_Filename,FileMode.OpenOrCreate,FileAccess.Read))
         {
            if(stream.Length > 0)
            {
               IFormatter formatter = new BinaryFormatter();
               m_PersistentInstances = formatter.Deserialize(stream) as ConcurrentDictionary<ActorId,ActorInfo>;
            }
         }
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      public static void UpdateIdleTime(ActorId actorId)
      {
         if(m_VolatileInstances.ContainsKey(actorId))
         {
            m_VolatileInstances[actorId].IdleStartTime = DateTime.Now;
         }
         else
         {
            if(m_PersistentInstances.ContainsKey(actorId))
            {
               m_PersistentInstances[actorId].IdleStartTime = DateTime.Now;
            }
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public static Guid GetInstance(ActorId actorId)
      {
         Guid instanceId = Guid.Empty;
         if(m_VolatileInstances.ContainsKey(actorId))
         {
            instanceId = m_VolatileInstances[actorId].DurableInstanceId;
         }
         else
         {
            if(m_PersistentInstances.ContainsKey(actorId))
            {
               instanceId = m_PersistentInstances[actorId].DurableInstanceId;
            }
         }
         return instanceId;
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public static void SaveInstance(ActorId actorId,ActorInfo state,bool persistent)
      {
         if(persistent)
         {
            SaveInstance(actorId,state,true,m_PersistentInstances);
         }
         else
         {
            SaveInstance(actorId,state,false,m_VolatileInstances);
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      internal static void RemoveInstance(ActorId actorId)
      {
         ActorInfo state = null;
         if(m_VolatileInstances.ContainsKey(actorId))
         {
            m_VolatileInstances.TryRemove(actorId,out state);
         }
         else
         {
            if(m_PersistentInstances.ContainsKey(actorId))
            {
               m_PersistentInstances.TryRemove(actorId,out state);
               SaveFile(m_PersistentInstances);
            }
         }
      }

      static void DeletePersistentInstances()
      {
         string connnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DurableServices"].ConnectionString;
         System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connnectionString);
         if(connection != null)
         {
            connection.Open();
            System.Data.SqlClient.SqlCommand command = connection.CreateCommand();
            command.CommandText = "DELETE From InstanceData";
            command.ExecuteNonQuery();
            connection.Close();
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public static void PurgeState()
      {
         DeletePersistentInstances();
         m_VolatileInstances.Clear();
         m_PersistentInstances.Clear();
         SaveFile(m_PersistentInstances);
      }
   }
}