// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Security;
using System.Web.Compilation;
using System.Web.Security;

using Microsoft.ServiceBus;
using ServiceModelEx.ServiceBus;

namespace ServiceModelEx
{
   class SecurityBehavior : IServiceBehavior
   {
      readonly ServiceSecurity m_Mode;
      readonly StoreLocation m_StoreLocation;
      readonly StoreName m_StoreName;
      readonly X509FindType m_FindType;
      readonly string m_SubjectName;

      bool m_UseAspNetProviders;
      string m_ApplicationName = String.Empty;


      public bool ImpersonateAll
      {
         get; set;
      }
      public string ApplicationName
      {
         get
         {
            return m_ApplicationName;
         }
         set
         {
            m_ApplicationName = value;
         }
      }
      public bool UseAspNetProviders
      {
         get
         {
            return m_UseAspNetProviders;
         }
         set
         {
            m_UseAspNetProviders = value;
            if(value == true && Roles.Enabled == false)
            {
               EnableRoleManager();
            }
         }
      }
      /// <summary>
      /// </summary>
      /// <param name="mode">If set to ServiceSecurity.Anonymous,ServiceSecurity.BusinessToBusiness or ServiceSecurity.Internet then the service certificate must be listed in config file</param>
      public SecurityBehavior(ServiceSecurity mode) : this(mode,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,null)
      {}
      /// <summary>
      /// </summary>
      /// <param name="mode">Certificate is looked up by name from LocalMachine/My store</param>
      public SecurityBehavior(ServiceSecurity mode,string serviceCertificateName) : this(mode,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,serviceCertificateName)
      {}
      public SecurityBehavior(ServiceSecurity mode,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string subjectName)
      {
         m_Mode = mode;
         m_StoreLocation = storeLocation;
         m_StoreName = storeName;
         m_FindType = findType;
         m_SubjectName = subjectName;
      }

      public void Validate(ServiceDescription description,ServiceHostBase serviceHostBase)
      {
         if(m_SubjectName != null)
         {
            switch(m_Mode)
            {
               case ServiceSecurity.Anonymous:
               case ServiceSecurity.BusinessToBusiness:
               case ServiceSecurity.Internet:
               {
                  string subjectName;
                  if(m_SubjectName != String.Empty)
                  {
                     subjectName = m_SubjectName;
                  }
                  else
                  {
                     subjectName = description.Endpoints[0].Address.Uri.Host;
                  }
                  serviceHostBase.Credentials.ServiceCertificate.SetCertificate(m_StoreLocation,m_StoreName,m_FindType,subjectName);
                  break;
               }
               case ServiceSecurity.ServiceBus:
               {
                  string subjectName;
                  if(m_SubjectName != String.Empty)
                  {
                     subjectName = m_SubjectName;
                  }
                  else
                  {
                     subjectName = ServiceBusHelper.ExtractNamespace(description.Endpoints[0].Address.Uri);
                  }

                  serviceHostBase.Credentials.ServiceCertificate.SetCertificate(m_StoreLocation,m_StoreName,m_FindType,subjectName);

                  break;
               }
            }
         }
         else
         {
            switch(m_Mode)
            {
               case ServiceSecurity.Anonymous:
               case ServiceSecurity.BusinessToBusiness:
               case ServiceSecurity.Internet:
               {
                  string subjectName = description.Endpoints[0].Address.Uri.Host;
                  serviceHostBase.Credentials.ServiceCertificate.SetCertificate(m_StoreLocation,m_StoreName,m_FindType,subjectName);
                  break;
               }
               case ServiceSecurity.ServiceBus:
               {
                  string subjectName = ServiceBusHelper.ExtractNamespace(description.Endpoints[0].Address.Uri);
                  serviceHostBase.Credentials.ServiceCertificate.SetCertificate(m_StoreLocation,m_StoreName,m_FindType,subjectName);
                  break;
               }
            }
         }
         if(UseAspNetProviders == true)
         {
            Debug.Assert(serviceHostBase.Credentials != null);
            serviceHostBase.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseAspNetRoles;
            string applicationName;
            Debug.Assert(Roles.ApplicationName == Membership.ApplicationName);
            if(String.IsNullOrEmpty(ApplicationName))
            {
               ApplicationName = Membership.ApplicationName;
            }
            if(String.IsNullOrEmpty(ApplicationName) || ApplicationName == "/")
            {
               if(String.IsNullOrEmpty(Assembly.GetEntryAssembly().GetName().Name))
               {
                  applicationName = AppDomain.CurrentDomain.FriendlyName;
               }
               else
               {
                  applicationName = Assembly.GetEntryAssembly().GetName().Name;
               }
            }
            else
            {
               applicationName = ApplicationName;
            }
            Membership.ApplicationName = applicationName;
            Roles.ApplicationName = applicationName;

            if(m_Mode == ServiceSecurity.Internet)
            {
               serviceHostBase.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.MembershipProvider;
            }
         }
         else
         {
            Debug.Assert(m_ApplicationName == null);
            //Reiterate the defaults 
            serviceHostBase.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Windows;
            serviceHostBase.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;
         }
         if(m_Mode == ServiceSecurity.Anonymous || m_Mode == ServiceSecurity.BusinessToBusiness && UseAspNetProviders == false)
         {
            serviceHostBase.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.None;
         }

         //Make it affect only when possible 
         if(ImpersonateAll)
         {
            if(m_Mode == ServiceSecurity.Intranet || (m_Mode == ServiceSecurity.Internet && UseAspNetProviders == false))
            {
               return;
            }
            else
            {
               ImpersonateAll = false;
            }
         }
         if(m_Mode == ServiceSecurity.BusinessToBusiness)
         {
            serviceHostBase.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
         }
      }

      public void AddBindingParameters(ServiceDescription description,ServiceHostBase serviceHostBase,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
      {
         if(ImpersonateAll)
         {
            Debug.Assert(description == serviceHostBase.Description);
            serviceHostBase.ImpersonateAll();
         }
         switch(m_Mode)
         {
            case ServiceSecurity.None:
            {
               ConfigureNone(endpoints);
               break;
            }
            case ServiceSecurity.Anonymous:
            {
               ConfigureAnonymous(endpoints);
               break;
            }
            case ServiceSecurity.BusinessToBusiness:
            {
               ConfigureBusinessToBusiness(endpoints);
               break;
            }
            case ServiceSecurity.Internet:
            {
               ConfigureInternet(endpoints,UseAspNetProviders);
               break;
            }
            case ServiceSecurity.Intranet:
            {
               ConfigureIntranet(endpoints);
               break;
            }
            case ServiceSecurity.ServiceBus:
            {
               ConfigureServiceBus(endpoints);
               break;
            }
            default:
            {
               throw new InvalidOperationException(m_Mode + " is unrecognized security mode");
            }
         }
      }
      public void ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase serviceHostBase)
      {}
      internal static void ConfigureNone(IEnumerable<ServiceEndpoint> endpoints)
      {
         foreach (ServiceEndpoint endpoint in endpoints)
         {
            Binding binding = endpoint.Binding;

            if(binding is BasicHttpBinding)
            {
               BasicHttpBinding basicBinding = (BasicHttpBinding)binding;
               basicBinding.Security.Mode = BasicHttpSecurityMode.None;
               continue;
            }
            if(binding is NetTcpBinding)
            {
               NetTcpBinding tcpBinding = (NetTcpBinding)binding;
               tcpBinding.Security.Mode = SecurityMode.None;
               continue;
            }
            if(binding is NetNamedPipeBinding)
            {
               NetNamedPipeBinding pipeBinding = (NetNamedPipeBinding)binding;
               pipeBinding.Security.Mode = NetNamedPipeSecurityMode.None;
               continue;
            }
            if(binding is WSHttpBinding)
            {
               WSHttpBinding wsBinding = (WSHttpBinding)binding;
               wsBinding.Security.Mode = SecurityMode.None;
               continue;
            }
            if(binding is WSDualHttpBinding)
            {
               WSDualHttpBinding wsDualBinding = (WSDualHttpBinding)binding;
               wsDualBinding.Security.Mode = WSDualHttpSecurityMode.None;
               continue;
            }
            if(binding is NetMsmqBinding)
            {
               NetMsmqBinding msmqBinding = (NetMsmqBinding)binding;
               msmqBinding.Security.Mode = NetMsmqSecurityMode.None;
               continue;
            }
            if(endpoint.Contract.ContractType == typeof(IMetadataExchange))
            {
               Trace.WriteLine("No declarative security for MEX endpoint");
               continue;
            }
            if(endpoint is DiscoveryEndpoint)
            {
               Trace.WriteLine("No declarative security for discovery endpoint");
               continue;
            }
            throw new InvalidOperationException(binding.GetType() + " is unsupported with ServiceSecurity.None");
         }
      }
      internal static void ConfigureAnonymous(IEnumerable<ServiceEndpoint> endpoints)
      {
         foreach (ServiceEndpoint endpoint in endpoints)
         {
            Binding binding = endpoint.Binding;

            if(binding is NetTcpBinding)
            {
               NetTcpBinding tcpBinding = (NetTcpBinding)binding;
               tcpBinding.Security.Mode = SecurityMode.Message;
               tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
               continue;
            }
            if(binding is WSHttpBinding)
            {
               WSHttpBinding wsBinding = (WSHttpBinding)binding;
               wsBinding.Security.Mode = SecurityMode.Message;
               wsBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
               continue;
            }
            if(binding is WSDualHttpBinding)
            {
               WSDualHttpBinding wsDualBinding = (WSDualHttpBinding)binding;
               wsDualBinding.Security.Mode = WSDualHttpSecurityMode.Message;
               wsDualBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
               continue;
            }
            if(binding is NetMsmqBinding)
            {
               NetMsmqBinding msmqBinding = (NetMsmqBinding)binding;
               msmqBinding.Security.Mode = NetMsmqSecurityMode.Message;
               msmqBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
               continue;
            }
            if(endpoint.Contract.ContractType == typeof(IMetadataExchange))
            {
               Trace.WriteLine("No declarative security for MEX endpoint");
               continue;
            }
            if(endpoint is DiscoveryEndpoint)
            {
               Trace.WriteLine("No declarative security for discovery endpoint");
               continue;
            }
            throw new InvalidOperationException(binding.GetType() + " is unsupported with ServiceSecurity.Anonymous");
         }
      }
      internal static void ConfigureBusinessToBusiness(IEnumerable<ServiceEndpoint> endpoints)
      {
         foreach (ServiceEndpoint endpoint in endpoints)
         {
            Binding binding = endpoint.Binding;

            if(binding is BasicHttpBinding)
            {
               BasicHttpBinding basicBinding = (BasicHttpBinding)binding;
               basicBinding.Security.Mode = BasicHttpSecurityMode.Message;
               basicBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
               continue;
            }
            if(binding is WSHttpBinding)
            {
               WSHttpBinding wsBinding = (WSHttpBinding)binding;
               wsBinding.Security.Mode = SecurityMode.Message;
               wsBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
               continue;
            }
            if(binding is WSDualHttpBinding)
            {
               WSDualHttpBinding wsDualBinding = (WSDualHttpBinding)binding;
               wsDualBinding.Security.Mode = WSDualHttpSecurityMode.Message;
               wsDualBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
               continue;
            }
            if(endpoint.Contract.ContractType == typeof(IMetadataExchange))
            {
               Trace.WriteLine("No declarative security for MEX endpoint");
               continue;
            }
            if(endpoint is DiscoveryEndpoint)
            {
               Trace.WriteLine("No declarative security for discovery endpoint");
               continue;
            }
            throw new InvalidOperationException(binding.GetType() + " is unsupported with ServiceSecurity.BusinessToBusiness");
         }
      }
      internal static void ConfigureInternet(IEnumerable<ServiceEndpoint> endpoints,bool useAspNetProviders)
      {
         foreach (ServiceEndpoint endpoint in endpoints)
         {
            Binding binding = endpoint.Binding;

            if(binding is WSHttpBinding)
            {
               WSHttpBinding wsBinding = (WSHttpBinding)binding;
               wsBinding.Security.Mode = SecurityMode.Message;
               wsBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
               continue;
            }
            if(binding is WSDualHttpBinding)
            {
               WSDualHttpBinding wsDualBinding = (WSDualHttpBinding)binding;
               wsDualBinding.Security.Mode = WSDualHttpSecurityMode.Message;
               wsDualBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
               continue;
            }
            if(binding is NetTcpBinding)
            {
               Debug.Assert(useAspNetProviders == true,"You should be using Windows security");
               NetTcpBinding tcpBinding = (NetTcpBinding)binding;
               tcpBinding.Security.Mode = SecurityMode.Message;
               tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
               continue;
            }
            if(endpoint.Contract.ContractType == typeof(IMetadataExchange))
            {
               Trace.WriteLine("No declarative security for MEX endpoint");
               continue;
            }
            if(endpoint is DiscoveryEndpoint)
            {
               Trace.WriteLine("No declarative security for discovery endpoint");
               continue;
            }
            throw new InvalidOperationException(binding.GetType() + " is unsupported with ServiceSecurity.Internet");
         }
      }
      internal static void ConfigureIntranet(IEnumerable<ServiceEndpoint> endpoints)
      {
         foreach (ServiceEndpoint endpoint in endpoints)
         {
            Binding binding = endpoint.Binding;

            if(binding is NetTcpBinding)
            {
               NetTcpBinding tcpBinding = (NetTcpBinding)binding;
               tcpBinding.Security.Mode = SecurityMode.Transport;
               tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows; ;
               tcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
               continue;
            }
            if(binding is NetNamedPipeBinding)
            {
               NetNamedPipeBinding pipeBinding = (NetNamedPipeBinding)binding;
               pipeBinding.Security.Mode = NetNamedPipeSecurityMode.Transport;
               pipeBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
               continue;
            }
            if(binding is NetMsmqBinding)
            {
               NetMsmqBinding msmqBinding = (NetMsmqBinding)binding;
               msmqBinding.Security.Mode = NetMsmqSecurityMode.Transport;
               msmqBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.WindowsDomain;
               msmqBinding.Security.Transport.MsmqProtectionLevel = ProtectionLevel.EncryptAndSign;
               continue;
            }
            if(endpoint.Contract.ContractType == typeof(IMetadataExchange))
            {
               Trace.WriteLine("No declarative security for MEX endpoint");
               continue;
            }
            if(endpoint is DiscoveryEndpoint)
            {
               Trace.WriteLine("No declarative security for discovery endpoint");
               continue;
            }
            throw new InvalidOperationException(binding.GetType() + " is unsupported with ServiceSecurity.Intranet");
         }
      }
      internal static void ConfigureServiceBus(IEnumerable<ServiceEndpoint> endpoints)
      {
         foreach (ServiceEndpoint endpoint in endpoints)
         {
            Binding binding = endpoint.Binding;
            if(binding is NetTcpRelayBinding || binding is NetOnewayRelayBinding)
            {
               ServiceBusHelper.ConfigureBinding(binding);
               continue;
            }
            throw new InvalidOperationException(binding.GetType() + " is unsupported with ServiceSecurity.ServiceBus");
         }
      }
      internal static void EnableRoleManager()
      {
         try
         {
            //This code requires .NET 4.0
            PropertyInfo property = typeof(BuildManager).GetProperty("PreStartInitStage",BindingFlags.NonPublic | BindingFlags.Static);
            property.SetValue(null,1,null);
            Roles.Enabled = true;

            //Use this code on .NET 3.0 or 3.5
            /**********************
            //Get the role manager section.
            RoleManagerSection roleManagerSection = (RoleManagerSection)ConfigurationManager.GetSection("system.web/roleManager");

            //Use reflection to enable changing it
            FieldInfo fieldInfo = typeof(ConfigurationElement).GetField("_bReadOnly",BindingFlags.Instance|BindingFlags.NonPublic);
            fieldInfo.SetValue(roleManagerSection,false);

            roleManagerSection.Enabled = true;
             **********************/
         }
         catch
         {}
      }
   }
}