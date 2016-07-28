// © 2009 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Linq;
using System.ServiceModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Net;
using System.Messaging;
using System.Transactions;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Security;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Data.SqlClient;

namespace ServiceModelEx
{
   public enum StandardPermissionSet
   {
      Internet,
      LocalIntranet,
      FullTrust,
      Execution,
      SkipVerification
   }
   public static class CodeAccessSecurityHelper
   {
      //PermissionSet extenssions
      public static IEnumerable<IPermission> Convert(this PermissionSet permissionSet)
      {
         foreach(IPermission permission in permissionSet)
         {
            yield return permission;
         }
      }
      public static PermissionSet Intersect(this PermissionSet permissionSet,IPermission permission)
      {
         PermissionSet singlePermissionSet = new PermissionSet(PermissionState.None);
         singlePermissionSet.AddPermission(permission);
         return permissionSet.Intersect(singlePermissionSet); 
      }
      public static void AddPermissions(this PermissionSet permissionSet,PermissionSet otherPermissionSet)
      {
         foreach(IPermission permission in otherPermissionSet)
         {
            permissionSet.AddPermission(permission);
         }
      }
      public static void AddPermissions(this PermissionSet permissionSet,IEnumerable<IPermission> otherPermissions)
      {
         foreach(IPermission permission in otherPermissions)
         {
            permissionSet.AddPermission(permission);
         }
      }
      
      //Public helper methods 
      public static PermissionSet Convert(this IEnumerable<IPermission> permissions)
      {
         PermissionSet permissionSet = new PermissionSet(PermissionState.None);

         foreach(IPermission permission in permissions)
         {
            permissionSet.AddPermission(permission);
         }
         return permissionSet;
      }
      public static void SetPermissionsSet(this AppDomain appDomain,PermissionSet permissions)
      {
         //PolicyLevel policy = PolicyLevel.CreateAppDomainLevel();
         //policy.RootCodeGroup.PolicyStatement = new PolicyStatement(permissions);
         //appDomain.SetAppDomainPolicy(policy);
      }
      public static void DemandFullTrust()
      {
         PermissionSet permissions = PermissionSetFromStandardSet(StandardPermissionSet.FullTrust);
         permissions.Demand();
      }
      public static StrongName StrongnameFromType(Type type)
      {
         Assembly assembly = type.Assembly;
         string name = type.Name;
         Version version = assembly.GetName().Version;
         StrongNamePublicKeyBlob strongName = new StrongNamePublicKeyBlob(assembly.GetName().GetPublicKey());

         return new StrongName(strongName,name,version);
      }
      public static PermissionSet PermissionSetFromStandardSet(StandardPermissionSet standardSet)
      {
         PermissionSetAttribute attribute = new PermissionSetAttribute(SecurityAction.Demand);
         attribute.Name = standardSet.ToString();
         return attribute.CreatePermissionSet();
      }
      public static PermissionSet PermissionSetFromFile(string fileName)
      {
         PermissionSetAttribute attribute = new PermissionSetAttribute(SecurityAction.Demand);
         attribute.File = fileName;
         return attribute.CreatePermissionSet();
      }

      public static void DemandHostPermissions(this ServiceHost host)
      {
         foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
         {
            DemandHostConnectionPermissions(endpoint);
            DemandHostSecurityPermissions(endpoint);
            using(TransactionScope scope = new TransactionScope())
            {
               DemandTransactionPermissions(endpoint);
            }
         }
         DemandHostStorePermissions(host);
         DemandPerformanceCounterPermissions();
         DemandTracingPermissions();
         DemanAspNetProvidersPermissions(host);
      }
      
      public static void DemandClientPermissions<T>(this ClientBase<T> proxy,string operationName) where T : class
      {
         DemandClientConnectionPermissions(proxy.Endpoint);
         DemandTransactionPermissions(proxy.Endpoint,operationName);
         DemandTracingPermissions();
         DemandClientSecurityPermissions(proxy);
         DemandEnvironmentPermissions(proxy);
         DemandClientStorePermissions(proxy.Endpoint);
      }
      
      internal static void DemandClientStorePermissions(ServiceEndpoint endpoint)
      {
         if(MessageSecurityEnabled(endpoint) == false && WindowsSecurityEnabled(endpoint) == true)
         {
            return;
         }

         IPermission certPermission = new StorePermission(StorePermissionFlags.EnumerateStores|StorePermissionFlags.OpenStore|StorePermissionFlags.EnumerateCertificates);

         if(ScopesCertificate(endpoint) || UsesCertificateClientCredentials(endpoint))
         {
            certPermission.Demand();
         }

         if(MessageSecurityEnabled(endpoint) && ValidatesCertificates(endpoint) && WindowsSecurityEnabled(endpoint) == false)
         {
            certPermission.Demand();
         }
      }
      internal static void DemandHostStorePermissions(ServiceHost host)
      {
         bool validates = ValidatesCertificates(host);
         bool demand = UsesCertificateServiceCredentials(host);

         foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
         {
            if(UsesCertificateClientCredentials(endpoint) && validates)
            {
               demand = true;
               break;
            }
            if(MessageSecurityEnabled(endpoint))
            {
               if(IsAnonymous(endpoint))
               {
                  demand = true;
                  break;
               }
               else
               {
                  if(WindowsSecurityEnabled(endpoint) == false)
                  {
                     demand = true;
                     break;
                  }
               }
            }
         }
         if(demand)
         {
            IPermission certPermission = new StorePermission(StorePermissionFlags.EnumerateStores|StorePermissionFlags.OpenStore|StorePermissionFlags.EnumerateCertificates);
            certPermission.Demand();
         }
      }
      internal static void DemandTransactionPermissions(ServiceEndpoint endpoint)
      {
         DemandTransactionPermissions(endpoint,null);
      }
      internal static void DemandTransactionPermissions(ServiceEndpoint endpoint,string operationName)
      {
         bool transactionFlow = false;
         bool flowOptionAllowed = false;

         if(endpoint.Binding is NetMsmqBinding)
         {
            NetMsmqBinding msmqBinding = endpoint.Binding as NetMsmqBinding;
            if(msmqBinding.Durable)
            {
               transactionFlow = true;
               if(Transaction.Current != null)
               {
                  flowOptionAllowed = true;
               }
            }
         }

         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding tcpBinding = endpoint.Binding as NetTcpBinding;
            transactionFlow = tcpBinding.TransactionFlow;
         }
         if(endpoint.Binding is NetNamedPipeBinding)
         {
            NetNamedPipeBinding ipcBinding = endpoint.Binding as NetNamedPipeBinding;
            transactionFlow = ipcBinding.TransactionFlow;
         }
         if(endpoint.Binding is WSHttpBinding)
         {
            WSHttpBinding wsBinding = endpoint.Binding as WSHttpBinding;
            transactionFlow = wsBinding.TransactionFlow;
         }
         if(endpoint.Binding is WSDualHttpBinding)
         {
            WSDualHttpBinding wsDualBinding = endpoint.Binding as WSDualHttpBinding;
            transactionFlow = wsDualBinding.TransactionFlow;
         }
         if(transactionFlow)
         {
            if(Transaction.Current != null)
            {
               //If operationName is null, then at least one operation needs to allow flow
               foreach(OperationDescription operation in endpoint.Contract.Operations)
               {
                  string name = operationName ?? operation.Name;
                  if(name != operation.Name)
                  {
                     continue;
                  }
                  TransactionFlowAttribute attribute = operation.Behaviors.Find<TransactionFlowAttribute>();
                  if(attribute == null)
                  {
                     continue;
                  }
                  if(attribute.Transactions != TransactionFlowOption.NotAllowed)
                  {
                     flowOptionAllowed = true;
                     break;
                  }
               }
               if(flowOptionAllowed)
               {
                  IPermission distributedTransactionPermission = new DistributedTransactionPermission(PermissionState.Unrestricted);
                  distributedTransactionPermission.Demand();
               }
            }
         }
      }
      internal static void DemandClientConnectionPermissions(ServiceEndpoint endpoint)
      {
         PermissionSet connectionSet = new PermissionSet(PermissionState.None);
         if(endpoint.Binding is NetTcpBinding)
         {
            connectionSet.AddPermission(new SocketPermission(NetworkAccess.Connect,TransportType.Tcp,endpoint.Address.Uri.Host,endpoint.Address.Uri.Port));
            connectionSet.AddPermission(new DnsPermission(PermissionState.Unrestricted));
         }
         if(endpoint.Binding is WebHttpBinding || endpoint.Binding is WSHttpBinding || endpoint.Binding is BasicHttpBinding || endpoint.Binding is WSDualHttpBinding)
         {
            connectionSet.AddPermission(new WebPermission(NetworkAccess.Connect,endpoint.Address.Uri.AbsoluteUri));
         }
         //On the client, demand hosting permission for duplex over HTTP
         if(endpoint.Binding is WSDualHttpBinding)
         {
            connectionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal));

            WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
            
            Uri callbackUri = binding.ClientBaseAddress ?? new Uri("http://localhost:80/");

            connectionSet.AddPermission(new WebPermission(NetworkAccess.Accept,callbackUri.AbsoluteUri));
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            string path = QueuedServiceHelper.GetQueueFromUri(endpoint.Address.Uri);
            connectionSet.AddPermission(new MessageQueuePermission(MessageQueuePermissionAccess.Send,path));
         }
         connectionSet.Demand();
      }
      internal static void DemandHostConnectionPermissions(ServiceEndpoint endpoint)
      {
         PermissionSet connectionSet = new PermissionSet(PermissionState.None);
         if(endpoint.Binding is NetTcpBinding)
         {
            connectionSet.AddPermission(new SocketPermission(NetworkAccess.Accept,TransportType.Tcp,endpoint.Address.Uri.Host,endpoint.Address.Uri.Port));
         }
         if(endpoint.Binding is WebHttpBinding || endpoint.Binding is WSHttpBinding || endpoint.Binding is BasicHttpBinding || endpoint.Binding is WSDualHttpBinding)
         {
            connectionSet.AddPermission(new WebPermission(NetworkAccess.Accept,endpoint.Address.Uri.AbsoluteUri));
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            string path = QueuedServiceHelper.GetQueueFromUri(endpoint.Address.Uri);
            connectionSet.AddPermission(new MessageQueuePermission(MessageQueuePermissionAccess.Receive,path));
         }
         connectionSet.Demand();
      }

      internal static void DemandEnvironmentPermissions<T>(ClientBase<T> proxy) where T : class
      {
         if(!SecurityEnabled(proxy.Endpoint) || IsAnonymous(proxy.Endpoint))
         {
            return;
         }
         IPermission permission = new EnvironmentPermission(EnvironmentPermissionAccess.Read,"USERNAME");
         IStackWalk environment = permission as IStackWalk;
         environment.Assert();
         string windowsUserName = proxy.ClientCredentials.Windows.ClientCredential.UserName;
         CodeAccessPermission.RevertAssert();

         if(windowsUserName == String.Empty && proxy.ClientCredentials.UserName.UserName == null)
         {
            Debug.Assert(WindowsSecurityEnabled(proxy.Endpoint));
            permission.Demand();
         }
      }

      [EnvironmentPermission(SecurityAction.Assert,Read = "USERNAME")]
      internal static void DemandClientSecurityPermissions<T>(ClientBase<T> proxy) where T : class
      {
         SecurityPermissionFlag flags = SecurityPermissionFlag.Execution;

         PermissionSet securitySet = new PermissionSet(PermissionState.None);

         if(proxy.Endpoint.Binding is NetNamedPipeBinding)
         {
            NetNamedPipeBinding ipcBinding = proxy.Endpoint.Binding as NetNamedPipeBinding;
            if(ipcBinding.Security.Mode == NetNamedPipeSecurityMode.Transport)
            {
               flags |= SecurityPermissionFlag.ControlEvidence|SecurityPermissionFlag.ControlPolicy;
            }
         }
         //Non-default Windows creds
         if(proxy.ClientCredentials.Windows.ClientCredential.UserName != String.Empty || proxy.ClientCredentials.UserName.UserName != null)
         {
            if(WindowsSecurityEnabled(proxy.Endpoint))
            {
               flags |= SecurityPermissionFlag.ControlPrincipal;
            }
         }
         if(proxy.Endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding tcpBinding = proxy.Endpoint.Binding as NetTcpBinding;
            if(tcpBinding.ReliableSession.Enabled)
            {
               flags |= SecurityPermissionFlag.ControlPolicy;
            }
            if(proxy is DuplexClientBase<T>)
            {
               flags |= SecurityPermissionFlag.ControlPolicy|SecurityPermissionFlag.ControlEvidence;
            }
         }
         if(MessageSecurityEnabled(proxy.Endpoint))
         {
            if(ValidatesCertificates(proxy.Endpoint) == false && ScopesCertificate(proxy.Endpoint) == false && WindowsSecurityEnabled(proxy.Endpoint) == false)
            {
               flags |= SecurityPermissionFlag.ControlPolicy|SecurityPermissionFlag.ControlEvidence;
            }
         }
         IPermission securityPermission = new SecurityPermission(flags);
         securityPermission.Demand();
      }

      internal static void DemandHostSecurityPermissions(ServiceEndpoint endpoint)
      {
         SecurityPermissionFlag flags = SecurityPermissionFlag.Execution;

         PermissionSet securitySet = new PermissionSet(PermissionState.None);

         if(endpoint.Binding is NetNamedPipeBinding)
         {
            NetNamedPipeBinding ipcBinding = endpoint.Binding as NetNamedPipeBinding;
            if(ipcBinding.Security.Mode == NetNamedPipeSecurityMode.Transport)
            {
               flags |= SecurityPermissionFlag.ControlEvidence|SecurityPermissionFlag.ControlPolicy;
            }
         }
         if(SecurityEnabled(endpoint) && IsAnonymous(endpoint) == false)
         {
            flags |= SecurityPermissionFlag.ControlPrincipal;
         }
         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding tcpBinding = endpoint.Binding as NetTcpBinding;
            if(tcpBinding.ReliableSession.Enabled)
            {
               flags |= SecurityPermissionFlag.ControlPolicy;
            }
         }
         IPermission securityPermission = new SecurityPermission(flags);
         securityPermission.Demand();
      }

      internal static void DemandAsyncPermissions()
      {
         IPermission permission = new SecurityPermission(SecurityPermissionFlag.ControlEvidence|SecurityPermissionFlag.ControlPolicy);
         permission.Demand();
      }
      static void DemanAspNetProvidersPermissions(ServiceHost host)
      {
         bool demand = false;

         foreach(IServiceBehavior behavior in host.Description.Behaviors)
         {
            if(behavior is ServiceCredentials)
            {
               ServiceCredentials credentialsBehavior = behavior as ServiceCredentials;
               if(credentialsBehavior.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.MembershipProvider)
               {
                  demand = true;
                   break;
               }
            }
            if(behavior is ServiceAuthorizationBehavior)
            {
               ServiceAuthorizationBehavior serviceAuthorization = behavior as ServiceAuthorizationBehavior;
               if(serviceAuthorization.PrincipalPermissionMode == PrincipalPermissionMode.UseAspNetRoles && Roles.Enabled)
               {
                  demand = true;
                  break;
               }
            }
         }
         if(demand)
         {
            IPermission permission = new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal);
            permission.Demand();
         }
      }
      [ConfigurationPermission(SecurityAction.Assert,Unrestricted = true)]
      internal static void DemandTracingPermissions()
      {
         PermissionSet tracingSet = new PermissionSet(PermissionState.None);

         tracingSet.AddPermission(ExtractPermissionFromTraceSource("System.ServiceModel.MessageLogging"));
         tracingSet.AddPermission(ExtractPermissionFromTraceSource("System.ServiceModel"));
         if(tracingSet.Count > 0)
         {
            tracingSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read,"COMPUTERNAME"));
         }

         tracingSet.Demand();
      }
      
      [FileIOPermission(SecurityAction.Assert,Unrestricted = true)]
      [ConfigurationPermission(SecurityAction.Assert,Unrestricted = true)]
      [EnvironmentPermission(SecurityAction.Assert,Unrestricted = true)]
      internal static void DemandPerformanceCounterPermissions()
      {
         Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ServiceModelSectionGroup sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);
         if(sectionGroup.Diagnostic.PerformanceCounters == PerformanceCounterScope.All || sectionGroup.Diagnostic.PerformanceCounters == PerformanceCounterScope.ServiceOnly)
         {
            PermissionSet countersPermissionSet = new PermissionSet(PermissionState.None);
            
            countersPermissionSet.AddPermission(new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Write,Environment.MachineName,"ServiceModelService 3.0.0.0"));
            countersPermissionSet.AddPermission(new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Write,Environment.MachineName,"ServiceModelEndpoint 3.0.0.0"));
            countersPermissionSet.AddPermission(new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Write,Environment.MachineName,"ServiceModelOperation 3.0.0.0"));

            if(sectionGroup.Diagnostic.PerformanceCounters == PerformanceCounterScope.All)
            {
               countersPermissionSet.AddPermission(new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Write,Environment.MachineName,"SMSvcHost 3.0.0.0"));
            }
            countersPermissionSet.Demand();
         }
      }


      //Private helper methods
      [ReflectionPermission(SecurityAction.Assert,Unrestricted = true)]
      [SecurityPermission(SecurityAction.Assert,Unrestricted = true)]
      static IPermission ExtractPermissionFromTraceSource(string sourceName)
      {
         IPermission permission = new FileIOPermission(PermissionState.None);

         TraceSource traceSource = new TraceSource(sourceName);
         try
         {
            if(traceSource.Listeners.Count == 0)
            {
               return permission;
            }
            foreach(TraceListener listener in traceSource.Listeners)
            {
               TextWriterTraceListener writer = listener as TextWriterTraceListener;
               if(writer != null)
               {
                  FieldInfo field = typeof(TextWriterTraceListener).GetField("fileName",BindingFlags.NonPublic|BindingFlags.Instance);
                  string fileName = field.GetValue(writer) as string;
                  permission = new FileIOPermission(FileIOPermissionAccess.Append|FileIOPermissionAccess.PathDiscovery|FileIOPermissionAccess.Write,fileName);
                  break;
               }
            }
            return permission;
         }
         catch
         {
            permission = new FileIOPermission(PermissionState.Unrestricted);
         }
         return permission;
      }

      static bool ValidatesCertificates(ServiceEndpoint endpoint)
      {
         foreach(IEndpointBehavior behavior in endpoint.Behaviors)
         {
            if(behavior is ClientCredentials)
            {
               ClientCredentials credentialsBehavior = behavior as ClientCredentials;
               return credentialsBehavior.ServiceCertificate.Authentication.CertificateValidationMode != X509CertificateValidationMode.None;
            }
         }
         return false;
      }
      static bool UsesCertificateServiceCredentials(ServiceHost host)
      {
         foreach(IServiceBehavior behavior in host.Description.Behaviors)
         {
            if(behavior is ServiceCredentials)
            {
               ServiceCredentials credentialsBehavior = behavior as ServiceCredentials;
               return credentialsBehavior.ServiceCertificate.Certificate != null;
            }
         }
         return false;
      }
      static bool ValidatesCertificates(ServiceHost host)
      {
         foreach(IServiceBehavior behavior in host.Description.Behaviors)
         {
            if(behavior is ServiceCredentials)
            {
               ServiceCredentials credentialsBehavior = behavior as ServiceCredentials;
               return credentialsBehavior.ClientCertificate.Authentication.CertificateValidationMode != X509CertificateValidationMode.None;
            }
         }
         return false;
      }
      static bool ScopesCertificate(ServiceEndpoint endpoint)
      {
         foreach(IEndpointBehavior behavior in endpoint.Behaviors)
         {
            if(behavior is ClientCredentials)
            {
               ClientCredentials credentialsBehavior = behavior as ClientCredentials;
               return credentialsBehavior.ServiceCertificate.ScopedCertificates.ContainsKey(endpoint.Address.Uri);
            }
         }
         return false;
      }

      static bool WindowsSecurityEnabled(ServiceEndpoint endpoint)
      {
         bool windowsSecurity = false;
         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding binding = endpoint.Binding as NetTcpBinding;
            bool windowsTransport = binding.Security.Mode == SecurityMode.Transport && binding.Security.Transport.ClientCredentialType == TcpClientCredentialType.Windows;
            bool windowsMessage = binding.Security.Mode == SecurityMode.Message && binding.Security.Message.ClientCredentialType == MessageCredentialType.Windows;
            windowsSecurity = windowsTransport|windowsMessage;
         }
         if(endpoint.Binding is NetNamedPipeBinding)
         {
            NetNamedPipeBinding binding = endpoint.Binding as NetNamedPipeBinding;
            windowsSecurity = binding.Security.Mode == NetNamedPipeSecurityMode.Transport;
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            NetMsmqBinding binding = endpoint.Binding as NetMsmqBinding;
            bool windowsTransport = binding.Security.Mode == NetMsmqSecurityMode.Transport && binding.Security.Transport.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain;
            bool windowsMessage = binding.Security.Mode == NetMsmqSecurityMode.Message && binding.Security.Message.ClientCredentialType == MessageCredentialType.Windows;
            windowsSecurity = windowsTransport||windowsMessage;
         }
         if(endpoint.Binding is WSHttpBinding)
         {
            WSHttpBinding binding = endpoint.Binding as WSHttpBinding;
            bool windowsTransport = binding.Security.Mode == SecurityMode.Transport && binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Windows;
            bool windowsMessage = binding.Security.Mode == SecurityMode.Message && binding.Security.Message.ClientCredentialType == MessageCredentialType.Windows;
            windowsSecurity = windowsTransport||windowsMessage;
         }
         if(endpoint.Binding is WSDualHttpBinding)
         {
            WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
            windowsSecurity = binding.Security.Mode == WSDualHttpSecurityMode.Message && binding.Security.Message.ClientCredentialType == MessageCredentialType.Windows;
         }
         if(endpoint.Binding is BasicHttpBinding)
         {
            BasicHttpBinding binding = endpoint.Binding as BasicHttpBinding;
            bool windowsTransport = binding.Security.Mode == BasicHttpSecurityMode.Transport && binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Windows;
            bool windowsTransportCredentialOnly = binding.Security.Mode == BasicHttpSecurityMode.TransportCredentialOnly && binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Windows;
            bool windowsTransportWithMessageCredential = binding.Security.Mode == BasicHttpSecurityMode.TransportWithMessageCredential && binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Windows;
            windowsSecurity = windowsTransport||windowsTransportCredentialOnly||windowsTransportWithMessageCredential;
         }
         if(endpoint.Binding is WebHttpBinding)
         {
            WebHttpBinding binding = endpoint.Binding as WebHttpBinding;
            windowsSecurity = binding.Security.Mode == WebHttpSecurityMode.Transport && binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Windows;
          }
         return windowsSecurity;
      }
      static bool UsesCertificateClientCredentials(ServiceEndpoint endpoint)
      {
         bool usesCertificates = false;
         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding binding = endpoint.Binding as NetTcpBinding;
            if(binding.Security.Mode == SecurityMode.Transport)
            {
               usesCertificates = binding.Security.Transport.ClientCredentialType == TcpClientCredentialType.Certificate;
            }
            if(binding.Security.Mode == SecurityMode.Message || binding.Security.Mode == SecurityMode.TransportWithMessageCredential)
            {
               usesCertificates = binding.Security.Message.ClientCredentialType == MessageCredentialType.Certificate;
            }
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            NetMsmqBinding binding = endpoint.Binding as NetMsmqBinding;
            if(binding.Security.Mode == NetMsmqSecurityMode.Transport || binding.Security.Mode == NetMsmqSecurityMode.Both)
            {
               usesCertificates = binding.Security.Transport.MsmqAuthenticationMode == MsmqAuthenticationMode.Certificate;
            }
            if(binding.Security.Mode == NetMsmqSecurityMode.Message || binding.Security.Mode == NetMsmqSecurityMode.Both)
            {
               usesCertificates = binding.Security.Message.ClientCredentialType == MessageCredentialType.Certificate;
            }
         }
         if(endpoint.Binding is WSHttpBinding)
         {
            WSHttpBinding binding = endpoint.Binding as WSHttpBinding;
            if(binding.Security.Mode == SecurityMode.Transport)
            {
               usesCertificates = binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Certificate;
            }
            if(binding.Security.Mode == SecurityMode.Message || binding.Security.Mode == SecurityMode.TransportWithMessageCredential)
            {
               usesCertificates = binding.Security.Message.ClientCredentialType == MessageCredentialType.Certificate;
            }
         }
         if(endpoint.Binding is WSDualHttpBinding)
         {
            WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
            if(binding.Security.Mode == WSDualHttpSecurityMode.Message)
            {
               usesCertificates = binding.Security.Message.ClientCredentialType == MessageCredentialType.None;
            }
         }
         if(endpoint.Binding is BasicHttpBinding)
         {
            BasicHttpBinding binding = endpoint.Binding as BasicHttpBinding;
            switch(binding.Security.Mode)
            {
               case BasicHttpSecurityMode.Message:
               {
                  usesCertificates = binding.Security.Message.ClientCredentialType == BasicHttpMessageCredentialType.Certificate;
                  break;
               }
               case BasicHttpSecurityMode.TransportCredentialOnly:
               case BasicHttpSecurityMode.TransportWithMessageCredential:
               case BasicHttpSecurityMode.Transport:
               {
                  usesCertificates = binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Certificate;
                  break;
               }
            }
         }
         if(endpoint.Binding is WebHttpBinding)
         {
            WebHttpBinding binding = endpoint.Binding as WebHttpBinding;
            if(binding.Security.Mode == WebHttpSecurityMode.Transport)
            {
               usesCertificates = binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.Certificate;
            }
         }
         return usesCertificates;
      }
      static bool SecurityEnabled(ServiceEndpoint endpoint)
      {
         bool noSecurity = false;
         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding binding = endpoint.Binding as NetTcpBinding;
            noSecurity = binding.Security.Mode == SecurityMode.None;
         }
         if(endpoint.Binding is NetNamedPipeBinding)
         {
            NetNamedPipeBinding binding = endpoint.Binding as NetNamedPipeBinding;
            noSecurity = binding.Security.Mode == NetNamedPipeSecurityMode.None;
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            NetMsmqBinding binding = endpoint.Binding as NetMsmqBinding;
            noSecurity = binding.Security.Mode == NetMsmqSecurityMode.None;
         }
         if(endpoint.Binding is WSHttpBinding)
         {
            WSHttpBinding binding = endpoint.Binding as WSHttpBinding;
            noSecurity = binding.Security.Mode == SecurityMode.None;
         }
         if(endpoint.Binding is WSDualHttpBinding)
         {
            WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
            noSecurity = binding.Security.Mode == WSDualHttpSecurityMode.None;
         }
         if(endpoint.Binding is BasicHttpBinding)
         {
            BasicHttpBinding binding = endpoint.Binding as BasicHttpBinding;
            noSecurity = binding.Security.Mode == BasicHttpSecurityMode.None;
         }
         if(endpoint.Binding is WebHttpBinding)
         {
            WebHttpBinding binding = endpoint.Binding as WebHttpBinding;
            noSecurity = binding.Security.Mode == WebHttpSecurityMode.None;
         }
         return ! noSecurity;
      }
      static bool IsAnonymous(ServiceEndpoint endpoint)
      {
         if(SecurityEnabled(endpoint) == false)
         {
            return true;
         }
         bool anonymous = false;

         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding binding = endpoint.Binding as NetTcpBinding;
            if(binding.Security.Mode == SecurityMode.Message)
            {
               anonymous = binding.Security.Message.ClientCredentialType == MessageCredentialType.None;
            }
            if(binding.Security.Mode == SecurityMode.Transport)
            {
               anonymous = binding.Security.Transport.ClientCredentialType == TcpClientCredentialType.None;
            }
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            NetMsmqBinding binding = endpoint.Binding as NetMsmqBinding;
            if(binding.Security.Mode == NetMsmqSecurityMode.Message)
            {
               anonymous = binding.Security.Message.ClientCredentialType == MessageCredentialType.None;
            }
            if(binding.Security.Mode == NetMsmqSecurityMode.Transport)
            {
               anonymous = binding.Security.Transport.MsmqAuthenticationMode == MsmqAuthenticationMode.None;
            }
         }
         if(endpoint.Binding is BasicHttpBinding)
         { 
            BasicHttpBinding binding = endpoint.Binding as BasicHttpBinding;
            if(binding.Security.Mode == BasicHttpSecurityMode.Transport)
            {
               anonymous = binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.None;
            }
         }
         if(endpoint.Binding is WSHttpBinding)
         { 
            WSHttpBinding binding = endpoint.Binding as WSHttpBinding;
            if(binding.Security.Mode == SecurityMode.Message)
            {
               anonymous = binding.Security.Message.ClientCredentialType == MessageCredentialType.None;
            }
            if(binding.Security.Mode == SecurityMode.Transport)
            {
               anonymous = binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.None;
            }
         }
         if(endpoint.Binding is WSDualHttpBinding)
         {
            WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
            if(binding.Security.Mode == WSDualHttpSecurityMode.Message)
            {
               anonymous = binding.Security.Message.ClientCredentialType == MessageCredentialType.None;
            }
         }
         if(endpoint.Binding is WebHttpBinding)
         {
            WebHttpBinding binding = endpoint.Binding as WebHttpBinding;
            anonymous = binding.Security.Transport.ClientCredentialType == HttpClientCredentialType.None;
         }         
         return anonymous;
      }

      static bool MessageSecurityEnabled(ServiceEndpoint endpoint)
      {
         if(endpoint.Binding is NetTcpBinding)
         {
            NetTcpBinding binding = endpoint.Binding as NetTcpBinding;
            return binding.Security.Mode == SecurityMode.Message;
         }
         if(endpoint.Binding is NetNamedPipeBinding)
         {
            return false;
         }
         if(endpoint.Binding is NetMsmqBinding)
         {
            NetMsmqBinding binding = endpoint.Binding as NetMsmqBinding;
            return binding.Security.Mode == NetMsmqSecurityMode.Message;
         }
         if(endpoint.Binding is WSHttpBinding)
         {
            WSHttpBinding binding = endpoint.Binding as WSHttpBinding;
            return binding.Security.Mode == SecurityMode.Message;
         }
         if(endpoint.Binding is WSDualHttpBinding)
         {
            WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
            return binding.Security.Mode == WSDualHttpSecurityMode.Message;
         }
         if(endpoint.Binding is BasicHttpBinding)
         {
            BasicHttpBinding binding = endpoint.Binding as BasicHttpBinding;
            return binding.Security.Mode == BasicHttpSecurityMode.Message;
         }
         return false;
      }

   }
}
