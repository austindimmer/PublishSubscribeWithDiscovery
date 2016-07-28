// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceModelEx
{
   public abstract class SecureDuplexClientBase<T,C> : DuplexClientBase<T,C> where T : class
   {
      //These constructors use the default endpoint
      protected SecureDuplexClientBase(C callback) : base(callback)
      {}
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureDuplexClientBase(ServiceSecurity mode,C callback) : base(callback)
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
      protected SecureDuplexClientBase(string userName,string password,C callback) : base(callback) 
      {
         this.SecureProxy(userName,password);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,C callback) : base(callback)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);

      }
      protected SecureDuplexClientBase(string domain,string userName,string password,C callback) : this(domain,userName,password,TokenImpersonationLevel.Identification,callback)
      {}
      protected SecureDuplexClientBase(string clientCertificateName,C callback) : base(callback) 
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureDuplexClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,C callback) : base(callback) 
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }
      protected SecureDuplexClientBase(InstanceContext<C> context) : base(context)
      {}
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureDuplexClientBase(ServiceSecurity mode,InstanceContext<C> context) : base(context)
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
      protected SecureDuplexClientBase(string userName,string password,InstanceContext<C> context) : base(context) 
      {
         this.SecureProxy(userName,password);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,InstanceContext<C> context) : base(context)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,InstanceContext<C> context) : this(domain,userName,password,TokenImpersonationLevel.Identification,context)
      {}
      protected SecureDuplexClientBase(string clientCertificateName,InstanceContext<C> context) : base(context) 
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureDuplexClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,InstanceContext<C> context) : base(context) 
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }

      //These constructors use configured endpoint

      protected SecureDuplexClientBase(C callback,string endpointName) : base(callback,endpointName)
      {}
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureDuplexClientBase(ServiceSecurity mode,C callback,string endpointName) : base(callback,endpointName)
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
      protected SecureDuplexClientBase(string userName,string password,C callback,string endpointName) : base(callback,endpointName)
      {
         this.SecureProxy(userName,password);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,C callback,string endpointName) : base(callback,endpointName)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);

      }
      protected SecureDuplexClientBase(string domain,string userName,string password,C callback,string endpointName) : this(domain,userName,password,TokenImpersonationLevel.Identification,callback,endpointName)
      {}
      protected SecureDuplexClientBase(string clientCertificateName,C callback,string endpointName) : base(callback,endpointName)
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureDuplexClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,C callback,string endpointName) : base(callback,endpointName)
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }
      protected SecureDuplexClientBase(InstanceContext<C> context,string endpointName) : base(context,endpointName)
      {}
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureDuplexClientBase(ServiceSecurity mode,InstanceContext<C> context,string endpointName) : base(context,endpointName)
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
      protected SecureDuplexClientBase(string userName,string password,InstanceContext<C> context,string endpointName) : base(context,endpointName)
      {
         this.SecureProxy(userName,password);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,InstanceContext<C> context,string endpointName) : base(context,endpointName)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,InstanceContext<C> context,string endpointName) : this(domain,userName,password,TokenImpersonationLevel.Identification,context,endpointName)
      {}
      protected SecureDuplexClientBase(string clientCertificateName,InstanceContext<C> context,string endpointName) : base(context,endpointName)
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureDuplexClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,InstanceContext<C> context,string endpointName) : base(context,endpointName) 
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }

      //These constructors use programatic address and binding
      
      protected SecureDuplexClientBase(C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {}
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureDuplexClientBase(ServiceSecurity mode,C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
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
      protected SecureDuplexClientBase(string userName,string password,C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {
         this.SecureProxy(userName,password);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);

      }
      protected SecureDuplexClientBase(string domain,string userName,string password,C callback,Binding binding,EndpointAddress remoteAddress) : this(domain,userName,password,TokenImpersonationLevel.Identification,callback,binding,remoteAddress)
      {}
      protected SecureDuplexClientBase(string clientCertificateName,C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureDuplexClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,C callback,Binding binding,EndpointAddress remoteAddress) : base(callback,binding,remoteAddress)
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }
      protected SecureDuplexClientBase(InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context,binding,remoteAddress)
      {}
      /// <summary>
      /// 
      /// </summary>
      //<param name="mode">Allowed values are ServiceSecurity.None and ServiceSecurity.Anonymous</param>
      protected SecureDuplexClientBase(ServiceSecurity mode,InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context,binding,remoteAddress)
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
      protected SecureDuplexClientBase(string userName,string password,InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context,binding,remoteAddress)
      {
         this.SecureProxy(userName,password);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,TokenImpersonationLevel impersonationLevel,InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context,binding,remoteAddress)
      {
         this.SecureProxy(domain,userName,password,impersonationLevel);
      }
      protected SecureDuplexClientBase(string domain,string userName,string password,InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : this(domain,userName,password,TokenImpersonationLevel.Identification,context,binding,remoteAddress)
      {}
      protected SecureDuplexClientBase(string clientCertificateName,InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context,binding,remoteAddress)
      {
         this.SecureProxy(clientCertificateName);
      }
      protected SecureDuplexClientBase(StoreLocation storeLocation,StoreName storeName,X509FindType findType,string clientCertificateName,InstanceContext<C> context,Binding binding,EndpointAddress remoteAddress) : base(context,binding,remoteAddress)
      {
         this.SecureProxy(storeLocation,storeName,findType,clientCertificateName);
      }
   }
}