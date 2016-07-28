// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;

namespace ServiceModelEx.ServiceFabric
{
   public static class BindingHelper
   {
      static NetNamedPipeContextBinding CreateBinding(string configName)
      {
         NetNamedPipeContextBinding binding;
         try
         {
            binding = new NetNamedPipeContextBinding(configName);
         }
         catch
         {
            binding = new NetNamedPipeContextBinding();
         }

         binding.Security.Mode = NetNamedPipeSecurityMode.None;
         binding.TransactionFlow = false;
         binding.MaxReceivedMessageSize = 4 * 1024 * 1024;
         binding.MaxBufferPoolSize = binding.MaxReceivedMessageSize * 4;
         binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
         binding.MaxConnections = int.MaxValue;
         if(Debugger.IsAttached)
         {
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
         }
         else
         {
            binding.OpenTimeout = TimeSpan.FromSeconds(5);
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.CloseTimeout = TimeSpan.FromSeconds(5);
         }
         return binding;
      }

      public static class Actor
      {
         static NetNamedPipeContextBinding EnforceActorBindingPolicy(string configName = null)
         {
            if(string.IsNullOrEmpty(configName))
            {
               return CreateBinding("ActorBinding");
            }
            else
            {
               return CreateBinding(configName);
            }
         }
         public static NetNamedPipeContextBinding Binding(string configName = null)
         {
            return EnforceActorBindingPolicy(configName);
         }
         public static NetNamedPipeContextBinding ProxyBinding(string configName = null)
         {
            NetNamedPipeContextBinding binding = EnforceActorBindingPolicy(configName);
            binding.MaxConnections = 48;
            return binding;
         }
      }
      public static class Service
      {
         public static class Default
         {
            static NetNamedPipeContextBinding EnforceServiceBindingPolicy(string configName = null)
            {
               if(string.IsNullOrEmpty(configName))
               {
                  return CreateBinding("ServiceBinding");
               }
               else
               {
                  return CreateBinding(configName);
               }
            }
            public static NetNamedPipeContextBinding Binding(string configName = null)
            {
               return EnforceServiceBindingPolicy(configName);
            }
            public static NetNamedPipeContextBinding ProxyBinding(string configName = null)
            {
               NetNamedPipeContextBinding binding = EnforceServiceBindingPolicy(configName);
               binding.MaxConnections = 48;
               return binding;
            }
         }
         public static class Wcf
         {
            static NetTcpBinding CreateBinding(string configName)
            {
               NetTcpBinding binding;
               try
               {
                  binding = new NetTcpBinding(configName);
               }
               catch
               {
                  binding = new NetTcpBinding(SecurityMode.Transport,true);
               }

               binding.TransactionFlow = false;
               binding.PortSharingEnabled = true;
               binding.MaxReceivedMessageSize = 4 * 1024 * 1024;
               binding.MaxBufferPoolSize = binding.MaxReceivedMessageSize * 4;
               binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
               binding.MaxConnections = int.MaxValue;
               if(Debugger.IsAttached)
               {
                  binding.OpenTimeout = TimeSpan.FromMinutes(1);
                  binding.SendTimeout = TimeSpan.FromMinutes(10);
                  binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                  binding.CloseTimeout = TimeSpan.FromMinutes(1);
               }
               else
               {
                  binding.OpenTimeout = TimeSpan.FromSeconds(5);
                  binding.SendTimeout = TimeSpan.MaxValue;
                  binding.ReceiveTimeout = TimeSpan.MaxValue;
                  binding.CloseTimeout = TimeSpan.FromSeconds(5);
               }
               return binding;
            }
            static NetTcpBinding EnforceServiceBindingPolicy(string configName = null)
            {
               if(string.IsNullOrEmpty(configName))
               {
                  return CreateBinding("WcfServiceBinding");
               }
               else
               {
                  return CreateBinding(configName);
               }
            }
            public static NetTcpBinding ServiceBinding(string configName = null)
            {
               return EnforceServiceBindingPolicy(configName);
            }
            public static NetTcpBinding ProxyBinding(string configName = null)
            {
               NetTcpBinding binding = EnforceServiceBindingPolicy(configName);
               binding.MaxConnections = 48;
               return binding;
            }
         }
      }
   }
}
