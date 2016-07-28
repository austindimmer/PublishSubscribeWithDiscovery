// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.IO;

namespace ServiceModelEx
{
   public static class GenericResolverInstaller
   {
      internal static Assembly CallingAssembly;

      internal static bool IsWebProcess()
      {
         if(Assembly.GetEntryAssembly() != null)
         {
            return false;
         }
         string processName = Process.GetCurrentProcess().ProcessName;

        return processName == "w3wp" || processName == "WebDev.WebServer40" || processName == "WaWorkerHost" || processName == "iisexpress";
      }

      internal static Assembly[] GetWebAssemblies()
      {
         Debug.Assert(IsWebProcess());
         List<Assembly> assemblies = new List<Assembly>();

         if(Assembly.GetEntryAssembly() != null)
         {  
            throw new InvalidOperationException("Can only call in a web assembly");
         }
         foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
         {
            var moduleName = assembly.ManifestModule.Name;
            if(moduleName.StartsWith("App_Code.") && moduleName.EndsWith(".dll"))
            {
               assemblies.Add(assembly);
            }
            if(moduleName.StartsWith("App_Web_") && moduleName.EndsWith(".dll"))
            {
               assemblies.Add(assembly);
            }
            if(assembly.Location.Contains(@"Temporary ASP.NET Files"))
            {
                assemblies.Add(assembly);
            }
         }
         if(assemblies.Count == 0)
         {
            string dynamicDirectory = AppDomain.CurrentDomain.DynamicDirectory;
            IEnumerable<string> files = Directory.EnumerateFiles(dynamicDirectory,"*.dll");
            foreach(string file in files)
            {
                assemblies.Add(Assembly.LoadFrom(file));
            }           
         }

         if(assemblies.Count == 0)
         {
            throw new InvalidOperationException("Could not find dynamic assembly");
         }
         return assemblies.ToArray();
      }
      
      [MethodImpl(MethodImplOptions.NoInlining)]
      public static void AddGenericResolver(this ServiceHost host,params Type[] typesToResolve)
      {
         CallingAssembly = Assembly.GetCallingAssembly();

         Debug.Assert(host.State != CommunicationState.Opened);

         foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
         {
            AddGenericResolver(endpoint,typesToResolve);
         }
      }
      [MethodImpl(MethodImplOptions.NoInlining)]
      public static void AddGenericResolver<T>(this ClientBase<T> proxy,params Type[] typesToResolve) where T : class
      {
         CallingAssembly = Assembly.GetCallingAssembly();

         Debug.Assert(proxy.State != CommunicationState.Opened);
         AddGenericResolver(proxy.Endpoint,typesToResolve);
      }
      [MethodImpl(MethodImplOptions.NoInlining)]
      public static void AddGenericResolver<T>(this ChannelFactory<T> factory,params Type[] typesToResolve) where T : class
      {
         CallingAssembly = Assembly.GetCallingAssembly();

         Debug.Assert(factory.State != CommunicationState.Opened);
         AddGenericResolver(factory.Endpoint,typesToResolve);
      }

      static void AddGenericResolver(ServiceEndpoint endpoint,Type[] typesToResolve)
      {
         foreach(OperationDescription operation in endpoint.Contract.Operations)
         {
            DataContractSerializerOperationBehavior behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            GenericResolver newResolver;

            if(typesToResolve == null || typesToResolve.Any() == false)
            {
               newResolver = new GenericResolver();
            }
            else
            {
               newResolver = new GenericResolver(typesToResolve);
            }

            GenericResolver oldResolver = behavior.DataContractResolver as GenericResolver;
            behavior.DataContractResolver = GenericResolver.Merge(oldResolver,newResolver);
         }
      }
   }
 }


   



