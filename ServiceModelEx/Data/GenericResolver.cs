// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;

namespace ServiceModelEx
{
   public class GenericResolver : DataContractResolver
   {
      const string DefaultNamespace = "global";

      readonly Dictionary<Type,Tuple<string,string>> m_TypeToNames;
      readonly Dictionary<string,Dictionary<string,Type>> m_NamesToType;

      public Type[] KnownTypes
      {
         get
         {
            return m_TypeToNames.Keys.ToArray();
         }
      }

      public GenericResolver() : this(ReflectTypes())
      {}

      public GenericResolver(Type[] typesToResolve)
      {
         m_TypeToNames = new Dictionary<Type,Tuple<string,string>>();
         m_NamesToType = new Dictionary<string,Dictionary<string,Type>>();

         foreach(Type type in typesToResolve)
         {
            string typeNamespace = GetNamespace(type);
            string typeName = GetName(type);

            m_TypeToNames[type] = new Tuple<string,string>(typeNamespace,typeName);

            if(m_NamesToType.ContainsKey(typeNamespace) == false)
            {
               m_NamesToType[typeNamespace] = new Dictionary<string,Type>();
            }
            m_NamesToType[typeNamespace][typeName] = type;
         }
      }

      public static GenericResolver Merge(GenericResolver resolver1,GenericResolver resolver2)
      {
         if(resolver1 == null)
         {
            return resolver2;
         }
         if(resolver2 == null)
         {
            return resolver1;
         }
         List<Type> types = new List<Type>();
         types.AddRange(resolver1.KnownTypes);
         types.AddRange(resolver2.KnownTypes);

         return new GenericResolver(types.ToArray());
      }
      static string GetNamespace(Type type)
      {
         return type.Namespace ?? DefaultNamespace;
      }

      static string GetName(Type type)
      {
         return type.Name;
      }
      
      public override Type ResolveName(string typeName,string typeNamespace,Type declaredType,DataContractResolver knownTypeResolver)
      {
         if(m_NamesToType.ContainsKey(typeNamespace))
         {
            if(m_NamesToType[typeNamespace].ContainsKey(typeName))
            {
               return m_NamesToType[typeNamespace][typeName];
            }
         }
         return knownTypeResolver.ResolveName(typeName,typeNamespace,declaredType,null);
      }
      public override bool TryResolveType(Type type,Type declaredType,DataContractResolver knownTypeResolver,out XmlDictionaryString typeName,out XmlDictionaryString typeNamespace)
      {
         if(m_TypeToNames.ContainsKey(type))
         {
            XmlDictionary dictionary = new XmlDictionary();
            typeNamespace = dictionary.Add(m_TypeToNames[type].Item1);
            typeName      = dictionary.Add(m_TypeToNames[type].Item2);

            return true;
         }
         else
         {
            return knownTypeResolver.TryResolveType(type,declaredType,null,out typeName,out typeNamespace);
         }
      }
   
      //Static helpers
      static Type[] ReflectTypes()
      {
         Assembly[] assemblyReferecnes = GetCustomReferencedAssemblies();

         List<Type> types = new List<Type>();
         
         foreach(Assembly assembly in assemblyReferecnes)
         {
            Type[] typesInReferencedAssembly = GetTypes(assembly);
            types.AddRange(typesInReferencedAssembly);
         }

         //MTM: Redundant?
         if(GenericResolverInstaller.CallingAssembly.FullName !=  typeof(ServiceHost).Assembly.FullName              &&
            GenericResolverInstaller.CallingAssembly.FullName !=  typeof(GenericResolverInstaller).Assembly.FullName)
         {
            Type[] typesInCallingAssembly = GetTypes(GenericResolverInstaller.CallingAssembly,false);
            types.AddRange(typesInCallingAssembly);
         }
         if(Assembly.GetEntryAssembly() != null)
         {
            if(Assembly.GetEntryAssembly().FullName != GenericResolverInstaller.CallingAssembly.FullName)
            {
               Type[] typesInEntryAssembly = GetTypes(Assembly.GetEntryAssembly(),false);
               types.AddRange(typesInEntryAssembly);
            }
         }
         else
         {
            if(GenericResolverInstaller.IsWebProcess())
            {
               foreach(Assembly assembly in GenericResolverInstaller.GetWebAssemblies())
               {
                  Type[] typesInWebAssembly = GetTypes(assembly,false);
                  types.AddRange(typesInWebAssembly);
               }
            }
            else
            {
               foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(domainAssembly=>IsSystemAssembly(domainAssembly.GetName()) == false))
               {
                  Type[] typesInDomainAssembly = GetTypes(assembly,false);
                  types.AddRange(typesInDomainAssembly);
               }               
            }
         }
         return types.ToArray();
      }

      static Assembly[] GetCustomReferencedAssemblies()
      {
         List<Assembly> assemblies = new List<Assembly>();

         if(GenericResolverInstaller.CallingAssembly.FullName !=  typeof(ServiceHost).Assembly.FullName              &&
            GenericResolverInstaller.CallingAssembly.FullName !=  typeof(GenericResolverInstaller).Assembly.FullName)
         {
            assemblies.AddRange(GetCustomReferencedAssemblies(GenericResolverInstaller.CallingAssembly));
         }
         if(Assembly.GetEntryAssembly() != null)
         {
            if(Assembly.GetEntryAssembly().FullName != GenericResolverInstaller.CallingAssembly.FullName)
            {
               assemblies.AddRange(GetCustomReferencedAssemblies(Assembly.GetEntryAssembly()));
            }
         }
         else
         {
            if(GenericResolverInstaller.IsWebProcess())
            {
               foreach(Assembly assembly in GenericResolverInstaller.GetWebAssemblies())
               {
                  assemblies.AddRange(GetCustomReferencedAssemblies(assembly));
               }
            }
            else
            {
               foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(domainAssembly=>IsSystemAssembly(domainAssembly.GetName()) == false))
               {
                  assemblies.AddRange(GetCustomReferencedAssemblies(assembly));
               }               
            }
         }
         return assemblies.ToArray();         
      }

      static bool IsSystemAssembly(AssemblyName assemblyName)
      {
         bool isSystemAssembly = false;

         const string dotNetKeyToken1 = "b77a5c561934e089";
         const string dotNetKeyToken2 = "b03f5f7f11d50a3a";
         const string dotNetKeyToken3 = "31bf3856ad364e35";

         string keyToken = assemblyName.FullName.Split('=')[3];

         Assembly serviceNModelEx = Assembly.GetCallingAssembly();
         string serviceNModelExKeyToken = serviceNModelEx.GetName().FullName.Split('=')[3];
         if(keyToken == serviceNModelExKeyToken)
         {
            isSystemAssembly = true;
         }
         switch(keyToken)
         {
            case dotNetKeyToken1:
            case dotNetKeyToken2:
            case dotNetKeyToken3:
            {
               isSystemAssembly = true;
               break;
            }
         }

         return isSystemAssembly;
      }

      static Assembly[] GetCustomReferencedAssemblies(Assembly assembly)
      {
         Debug.Assert(assembly != null);
         AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();

         List<Assembly> assemblies = new List<Assembly>();
         foreach(AssemblyName assemblyName in referencedAssemblies.Where(name=>IsSystemAssembly(name) == false))
         {
            assemblies.Add(Assembly.Load(assemblyName));
         }
         return assemblies.ToArray();
      }
      static Type[] GetTypes(Assembly assembly,bool publicOnly = true)
      {
         Type[] allTypes = assembly.GetTypes();

         List<Type> types = new List<Type>();

         foreach(Type type in allTypes)
         {
            if(type.IsEnum == false        && 
               type.IsInterface == false   && 
               type.IsGenericTypeDefinition == false)
            {
               if(publicOnly == true && type.IsPublic == false)
               {
                  if(type.IsNested == false)
                  {
                     continue;
                  }
                  if(type.IsNestedPrivate == true)
                  {
                     continue;
                  }
               }
               types.Add(type);
            }
         }
         return types.ToArray();
      }
   }
}