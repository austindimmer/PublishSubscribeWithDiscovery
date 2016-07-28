// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace ServiceModelEx
{  
   public abstract class SecureClientBase<T> : ClientBase<T> where T : class
   {
      protected SecureClientBase()
      {}
      //These constructors use the default endpoint
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureClientBase(ServiceSecurity mode)
      {
         switch(mode)
         {
            case ServiceSecurity.None:
            {
               this.UnsecuredProxy();
               break;
            }
            case ServiceSecurity.Anonymous:
            {
               this.AnonymousProxy();
               break;
            }
            default:
            {
               throw new InvalidOperationException(mode + " is unsupported with this constructor");
            }
         }
      }
      protected SecureClientBase(string userName,string password) 
      {
         this.SecureProxy(userName,password);
      }
      protected SecureClientBase(string userName,string password,Binding binding,EndpointAddress address) : base(binding,address)
      {
         this.SecureProxy(userName,password);
      }
      protected SecureClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);
      }
      protected SecureClientBase(string domain,string userName,string password) : this(domain,userName,password,TokenImpersonationLevel.Identification)
      {}
      protected SecureClientBase(string clientCertificateName) 
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName) 
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }


      //These constructors use configured endpoint
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureClientBase(ServiceSecurity mode,string endpointName) : base(endpointName)
      {
         switch(mode)
         {
            case ServiceSecurity.None:
            {
               SecurityHelper.UnsecuredProxy(this);
               break;
            }
            case ServiceSecurity.Anonymous:
            {
               SecurityHelper.AnonymousProxy(this);
               break;
            }
            default:
            {
               throw new InvalidOperationException(mode + " is unsupported with this constructor");
            }
         }
      }
      protected SecureClientBase(UserNamePasswordClientCredential credentials,string endpointName) : base(endpointName)
      {
         this.SecureProxy(credentials.UserName,credentials.Password);
      }
      protected SecureClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,string endpointName) : base(endpointName)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);
      }
      protected SecureClientBase(string domain,string userName,string password,string endpointName) : this(domain,userName,password,TokenImpersonationLevel.Identification,endpointName)
      {}

      protected SecureClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,string endpointName) : base(endpointName)
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }

   
      //These constructors use programatic address and binding

      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureClientBase(ServiceSecurity mode,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         switch(mode)
         {
            case ServiceSecurity.None:
            {
               SecurityHelper.UnsecuredProxy(this);
               break;
            }
            case ServiceSecurity.Anonymous:
            {
               SecurityHelper.AnonymousProxy(this);
               break;
            }
            default:
            {
               throw new InvalidOperationException(mode + " is unsupported with this constructor");
            }
         }
      }
      protected SecureClientBase(UserNamePasswordClientCredential credentials,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         this.SecureProxy(credentials.UserName,credentials.Password);
      }
      protected SecureClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);
      }
      protected SecureClientBase(string domain,string userName,string password,Binding binding,EndpointAddress remoteAddress) : this(domain,userName,password,TokenImpersonationLevel.Identification,binding,remoteAddress)
      {}

      protected SecureClientBase(string clientCertificateName,bool overrideConfig,string endpointName) : base(endpointName)
      {
         if(overrideConfig)
         {
            this.SecureProxy(clientCertificateName);
         }
      }
      protected SecureClientBase(string clientCertificateName,bool overrideConfig,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         if(overrideConfig)
         {
            this.SecureProxy(clientCertificateName);
         }
      }
      protected SecureClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,Binding binding,EndpointAddress remoteAddress) : base(binding,remoteAddress)
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }
   }
}