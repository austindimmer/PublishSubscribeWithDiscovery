// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace ServiceModelEx
{
   public static class SecurityHelper
   {
      public static void SetSecurityMode<T,C>(this DuplexChannelFactory<T,C> factory,ServiceSecurity mode) where T : class
      {
         switch(mode)
         {
            case ServiceSecurity.None:
            {
               if(factory.State == CommunicationState.Opened)
               {
                  throw new InvalidOperationException("Proxy channel is already opened");
               }
               ServiceEndpoint[] endpoints = {factory.Endpoint};

               SecurityBehavior.ConfigureNone(endpoints);

               break;
            }
            case ServiceSecurity.Anonymous:
            {
               if(factory.State == CommunicationState.Opened)
               {
                  throw new InvalidOperationException("Proxy channel is already opened");
               }
               ServiceEndpoint[] endpoints = {factory.Endpoint};

               SecurityBehavior.ConfigureAnonymous(endpoints);
                        
               factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

               break;
            }
            default:
            {
               throw new InvalidOperationException(mode + " is unsupported with this constructor");
            }
         }
      }
      public static void SetCredentials<T,C>(this DuplexChannelFactory<T,C> factory,string userName,string password) where T : class
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SecurityBehavior.ConfigureInternet(endpoints,true);//True even for Windows

         factory.Credentials.UserName.UserName = userName;
         factory.Credentials.UserName.Password = password;

         factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
      }
      public static void SetCredentials<T,C>(this DuplexChannelFactory<T,C> factory,string domain,string userName,string password,TokenImpersonationLevel impersonationLevel) where T : class
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SecurityBehavior.ConfigureIntranet(endpoints);

         NetworkCredential credentials = new NetworkCredential();
         credentials.Domain   = domain;
         credentials.UserName = userName;
         credentials.Password = password;

         factory.Credentials.Windows.ClientCredential = credentials;
         factory.Credentials.Windows.AllowedImpersonationLevel = impersonationLevel;
      }
      public static void SetCredentials<T,C>(this DuplexChannelFactory<T,C> factory,string domain,string userName,string password) where T : class 
      {
         SetCredentials(factory,domain,userName,password,TokenImpersonationLevel.Identification);
      }
      public static void SetCredentials<T,C>(this DuplexChannelFactory<T,C> factory,string clientCertificateName) where T : class 
      {
         SetCredentials(factory,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,clientCertificateName);
      }
      public static void SetCredentials<T,C>(this DuplexChannelFactory<T,C> factory,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName) where T : class 
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         factory.Credentials.ClientCertificate.SetCertificate(storeLocation,storeName,findType,clientCertificateName);
         
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SecurityBehavior.ConfigureBusinessToBusiness(endpoints);

         factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
      }   

      public static void SetSecurityMode<T>(this ChannelFactory<T> factory,ServiceSecurity mode)
      {
         switch(mode)
         {
            case ServiceSecurity.None:
            {
               if(factory.State == CommunicationState.Opened)
               {
                  throw new InvalidOperationException("Proxy channel is already opened");
               }
               ServiceEndpoint[] endpoints = {factory.Endpoint};

               SecurityBehavior.ConfigureNone(endpoints);

               break;
            }
            case ServiceSecurity.Anonymous:
            {
               if(factory.State == CommunicationState.Opened)
               {
                  throw new InvalidOperationException("Proxy channel is already opened");
               }
               ServiceEndpoint[] endpoints = {factory.Endpoint};

               SecurityBehavior.ConfigureAnonymous(endpoints);
               factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

               break;
            }
            default:
            {
               throw new InvalidOperationException(mode + " is unsupported with this constructor");
            }
         }
      }
      public static void SetCredentials<T>(this ChannelFactory<T> factory,string userName,string password) 
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SecurityBehavior.ConfigureInternet(endpoints,true);//True even for Windows

         factory.Credentials.UserName.UserName = userName;
         factory.Credentials.UserName.Password = password;
                  
         factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
      }
      public static void SetCredentials<T>(this ChannelFactory<T> factory,string domain,string userName,string password,TokenImpersonationLevel impersonationLevel)
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SecurityBehavior.ConfigureIntranet(endpoints);

         NetworkCredential credentials = new NetworkCredential();
         credentials.Domain   = domain;
         credentials.UserName = userName;
         credentials.Password = password;

         factory.Credentials.Windows.ClientCredential = credentials;
         factory.Credentials.Windows.AllowedImpersonationLevel = impersonationLevel;
      }
      public static void SetCredentials<T>(this ChannelFactory<T> factory,string domain,string userName,string password) 
      {
         SetCredentials(factory,domain,userName,password,TokenImpersonationLevel.Identification);
      }
      public static void SetCredentials<T>(this ChannelFactory<T> factory,string clientCertificateName) 
      {
         SetCredentials(factory,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,clientCertificateName);
      }
      public static void SetCredentials<T>(this ChannelFactory<T> factory,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName) 
      {
         if(factory.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         factory.Credentials.ClientCertificate.SetCertificate(storeLocation,storeName,findType,clientCertificateName);
         
         ServiceEndpoint[] endpoints = {factory.Endpoint};

         SecurityBehavior.ConfigureBusinessToBusiness(endpoints);
         factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
      }   


      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      /// <param name="mode">If set to ServiceSecurity.Anonymous,ServiceSecurity.BusinessToBusiness or ServiceSecurity.Internet then the service certificate must be listed in config file</param>
      public static void SetSecurityBehavior(this ServiceHost host,ServiceSecurity mode,bool useAspNetProviders,string applicationName,bool impersonateAll = false)
      {
         SetSecurityBehavior(host,mode,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,null,useAspNetProviders,applicationName,impersonateAll);
      }
      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      /// <param name="mode">Certificate is looked up by name from LocalMachine/My store</param>
      public static void SetSecurityBehavior(this ServiceHost host,ServiceSecurity mode,string serviceCertificateName,bool useAspNetProviders,string applicationName,bool impersonateAll = false) 
      {
         SetSecurityBehavior(host,mode,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,serviceCertificateName,useAspNetProviders,applicationName,impersonateAll);
      }

      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public static void SetSecurityBehavior(this ServiceHost host,ServiceSecurity mode,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string serviceCertificateName,bool useAspNetProviders,string applicationName,bool impersonateAll = false)
      {
         if(host.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }
         SecurityBehavior securityBehavior = new SecurityBehavior(mode,storeLocation,storeName,findType,serviceCertificateName);

         securityBehavior.UseAspNetProviders = useAspNetProviders;
         securityBehavior.ApplicationName = applicationName;
         securityBehavior.ImpersonateAll = impersonateAll;

         host.Description.Behaviors.Add(securityBehavior);
      }
      
      public static void ImpersonateAll(this ServiceHostBase host)
      {
         if(host.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }
         host.Authorization.ImpersonateCallerForAllOperations = true;
         host.Description.ImpersonateAll();
      }
      public static void ImpersonateAll(this ServiceDescription description)
      {
         foreach(ServiceEndpoint endpoint in description.Endpoints)
         {
            if(endpoint.Contract.Name == "IMetadataExchange")
            {
               continue;
            } 
            foreach(OperationDescription operation in endpoint.Contract.Operations)
            {
               OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();
               if(attribute != null)
               {
                  if(attribute.Impersonation == ImpersonationOption.NotAllowed)
                  {
                     Trace.WriteLine("Overriding impersonation setting of " + endpoint.Contract.Name + "." + operation.Name);
                  }
                  attribute.Impersonation = ImpersonationOption.Required; 
                  continue;
               }
            }
         }
      }

      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void UnsecuredProxy<T>(this ClientBase<T> proxy) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {proxy.Endpoint};

         SecurityBehavior.ConfigureNone(endpoints);
      }
      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void AnonymousProxy<T>(this ClientBase<T> proxy) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {proxy.Endpoint};
         proxy.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

         SecurityBehavior.ConfigureAnonymous(endpoints);
      }

      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void SecureProxy<T>(this ClientBase<T> proxy,string userName,string password) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {proxy.Endpoint};

         SecurityBehavior.ConfigureInternet(endpoints,true);//True even for Windows

         proxy.ClientCredentials.UserName.UserName = userName;
         proxy.ClientCredentials.UserName.Password = password;
         proxy.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
      }
            /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void SecureProxy<T>(this ClientBase<T> proxy,string domain,string userName,string password) where T : class
      {
         SecureProxy<T>(proxy,domain,userName,password,TokenImpersonationLevel.Identification);
      }
      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void SecureProxy<T>(this ClientBase<T> proxy,string domain,string userName,string password,TokenImpersonationLevel impersonationLevel) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         ServiceEndpoint[] endpoints = {proxy.Endpoint};

         SecurityBehavior.ConfigureIntranet(endpoints);

         NetworkCredential credentials = new NetworkCredential();
         credentials.Domain   = domain;
         credentials.UserName = userName;
         credentials.Password = password;

         proxy.ClientCredentials.Windows.ClientCredential = credentials;
         proxy.ClientCredentials.Windows.AllowedImpersonationLevel = impersonationLevel;
      }
      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      /// <param name="clientCertificateName">Certificate is looked up by name from LocalMachine/My store</param>
      public static void SecureProxy<T>(this ClientBase<T> proxy,string clientCertificateName) where T : class 
      {
         proxy.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
         SecureProxy<T>(proxy,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,clientCertificateName);
      }

      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void SecureProxy<T>(this ClientBase<T> proxy,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         SetCertificate(proxy,storeLocation,storeName,findType,clientCertificateName);
         ServiceEndpoint[] endpoints = {proxy.Endpoint};

         proxy.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

         SecurityBehavior.ConfigureBusinessToBusiness(endpoints);
      }
      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      /// <param name="clientCertificateName">Certificate is looked up by name from LocalMachine/My store</param>
      public static void SetCertificate<T>(this ClientBase<T> proxy,string clientCertificateName) where T : class
      {
         SetCertificate<T>(proxy,StoreLocation.LocalMachine,StoreName.My,X509FindType.FindBySubjectName,clientCertificateName);
      }

      /// <summary>
      /// Can only call before using the proxy for the first time
      /// </summary>
      public static void SetCertificate<T>(this ClientBase<T> proxy,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName) where T : class
      {
         if(proxy.State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Proxy channel is already opened");
         }
         if(String.IsNullOrEmpty(clientCertificateName) == false)
         {
            proxy.ClientCredentials.ClientCertificate.SetCertificate(storeLocation,storeName,findType,clientCertificateName);
         }
         proxy.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
      }
   }
}