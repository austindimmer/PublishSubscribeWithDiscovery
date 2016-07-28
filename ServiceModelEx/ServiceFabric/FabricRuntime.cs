// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

using ServiceModelEx.ServiceFabric;
using ServiceModelEx.ServiceFabric.Actors;
using ServiceModelEx.ServiceFabric.Services;
using ServiceModelEx.ServiceFabric.Services.Communication.Runtime;
using ServiceModelEx.ServiceFabric.Services.Communication.Wcf.Runtime;
using ServiceModelEx.ServiceFabric.Services.Runtime;

namespace ServiceModelEx.Fabric
{
   public sealed class FabricRuntime : IDisposable
   {
      internal static IEnumerable<Assembly> Assemblies
      {get;private set;}
      internal static Dictionary<string,Type[]> Services
      {get;set;}
      internal static Dictionary<string,Type[]> Actors
      {get;set;}
      static Dictionary<Type,ServiceHost> m_Hosts = new Dictionary<Type,ServiceHost>();
      static IEnumerable<Assembly> LoadAssemblies(string namespaceRoot)
      {
         List<Assembly> assemblies = new List<Assembly>();

         DirectoryInfo directories = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
         List<FileInfo> files = new List<FileInfo>(directories.GetFiles("*.dll"));
         files.AddRange(directories.GetFiles("*.exe"));
         files = new List<FileInfo>(files.Where(fileInfo=>!fileInfo.FullName.StartsWith("System") && (string.IsNullOrEmpty(namespaceRoot) ? true : fileInfo.Name.StartsWith(namespaceRoot))));

         foreach(FileInfo info in files)
         {
            try
            {
               assemblies.Add(Assembly.LoadFile(info.FullName));
            }
            catch
            {}
         }
         return assemblies.ToArray();
      }

      static readonly Type m_GenericServiceHostDefinition = null;

      public static FabricRuntime Create()
      {
         return Create(null);
      }
      public static FabricRuntime Create(string namespaceRoot = null)
      {
         FabricRuntime runtime = new FabricRuntime();
         Assemblies = LoadAssemblies(namespaceRoot);
         Services = new Dictionary<string,Type[]>();
         Actors = new Dictionary<string,Type[]>();
         FabricThreadPoolHelper.ConfigureThreadPool();
         return runtime;
      }
      public static void PurgeState()
      {
         ActorManager.PurgeState();
      }

      static FabricRuntime()
      {
         m_GenericServiceHostDefinition = typeof(ServiceHost<>).GetGenericTypeDefinition();
      }
      private FabricRuntime()
      {}
      public void Dispose()
      {}

      Type[] GetContracts(Type serviceType)
      {
         Type[] interfaces = serviceType.GetInterfaces();
         List<Type> contracts = new List<Type>();

         foreach(Type type in interfaces)
         {
            if(type.GetCustomAttributes(typeof(ServiceContractAttribute),false).Any())
            {
               contracts.Add(type);
            }
         }

         return contracts.ToArray();
      }
      void ValidateContract(Type serviceType,Type interfaceType)
      {
         Type[] contracts = GetContracts(serviceType);
         if(!contracts.Any(contractType=>contractType.Equals(interfaceType)))
         {
            throw new InvalidOperationException("Validation failed. Service Type " + serviceType.FullName + " does not implement interface type " + interfaceType.FullName + ".");
         }
      }
      void ActivateServiceType(Type serviceType)
      {
         StatelessService service = Activator.CreateInstance(serviceType) as StatelessService;
         foreach(ServiceInstanceListener instanceListener in service.ServiceInstanceListeners)
         {
            WcfCommunicationListener listener = instanceListener.CreateCommunicationListener(new StatelessServiceInitializationParameters()) as WcfCommunicationListener;
            if(listener != null)
            {
               if(listener.ImplementationType.Equals(serviceType) == false)
               {
                  throw new InvalidOperationException("Validation failed. Service Type " + serviceType.FullName + " does not match listener implementation type " + listener.ImplementationType.FullName + ".");
               }
               ValidateContract(serviceType,listener.InterfaceType);

               Type concreteHostType = m_GenericServiceHostDefinition.MakeGenericType(serviceType);
               ServiceHost host = Activator.CreateInstance(concreteHostType) as ServiceHost;

               NetTcpBinding binding = listener.Binding;
               if(binding == null)
               {
                  binding = BindingHelper.Service.Wcf.ServiceBinding();
               }

               host.Description.Behaviors.Add(new StatelessServiceBehavior());

               IEnumerable<ApplicationManifestAttribute> manifests = serviceType.GetCustomAttributes<ApplicationManifestAttribute>();
               foreach(ApplicationManifestAttribute manifest in manifests)
               {
                  host.AddServiceEndpoint(listener.InterfaceType,binding,AddressHelper.Wcf.BuildAddress("localhost",manifest.ApplicationName,manifest.ServiceName,listener.InterfaceType));
               }
               host.Open();
            }
         }
      }

      internal void RegisterServiceType(Dictionary<string,Type[]> applications,Type serviceType)
      {
         IEnumerable<ApplicationManifestAttribute> manifests = serviceType.GetCustomAttributes<ApplicationManifestAttribute>();
         Debug.Assert(manifests.Any());

         foreach (ApplicationManifestAttribute manifest in manifests)
         {
            Debug.Assert(manifest != null);
            Debug.Assert(!string.IsNullOrEmpty(manifest.ApplicationName));
            Debug.Assert(!string.IsNullOrEmpty(manifest.ServiceName));
            Debug.Assert(manifests.Count(attribute=>attribute.ApplicationName == manifest.ApplicationName) == 1);

            if(!applications.ContainsKey(manifest.ApplicationName))
            {
               applications.Add(manifest.ApplicationName,new Type[] {serviceType});
            }
            else
            {
               if(applications[manifest.ApplicationName].Any(type=>type.Equals(serviceType)))
               {
                  throw new InvalidOperationException("Validation failed. Service Type " + serviceType.FullName + " already exists in application " + manifest.ApplicationName + ".");
               }
               if(applications[manifest.ApplicationName].Any(type=>type.GetCustomAttributes<ApplicationManifestAttribute>().Any(m=>m.ServiceName.Equals(manifest.ServiceName))))
               {
                  throw new InvalidOperationException("Validation failed. Service Name " + manifest.ServiceName + " already exists in application " + manifest.ApplicationName + ".");
               }
               else
               {
                  List<Type> serviceTypes = new List<Type>(applications[manifest.ApplicationName]);
                  serviceTypes.Add(serviceType);
                  applications[manifest.ApplicationName] = serviceTypes.ToArray();
               }
            }
         }
      }
      public void RegisterServiceType(string serviceTypeName,Type serviceType,bool forTest = false)
      {
         Debug.Assert(serviceType.BaseType.Equals(typeof(StatelessService)));
         RegisterServiceType(FabricRuntime.Services,serviceType);
         if(forTest == false)
         {
            ActivateServiceType(serviceType);
         }
      }
   }
}
