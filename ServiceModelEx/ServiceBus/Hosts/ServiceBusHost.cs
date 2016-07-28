// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Web.Security;


namespace ServiceModelEx.ServiceBus
{
   public class ServiceBusHost : DiscoverableServiceHost
   {
      public ServiceBusHost(object singletonInstance,params Uri[] baseAddresses) : base(singletonInstance,baseAddresses)
      {}
      public ServiceBusHost(Type serviceType,params Uri[] baseAddresses) : base(serviceType,baseAddresses)
      {}

      protected override void OnOpening()
      {
         if(Credentials.ServiceCertificate.Certificate == null)
         {
            ConfigureAnonymousMessageSecurity();
         }
         base.OnOpening();
      }

      public void ConfigureAnonymousMessageSecurity(string serviceCert = "")
      {
         ConfigureAnonymousMessageSecurity(serviceCert,StoreLocation.LocalMachine,StoreName.My);
      }
      public void ConfigureAnonymousMessageSecurity(string serviceCert,StoreLocation location,StoreName storeName)
      {
         if(String.IsNullOrWhiteSpace(serviceCert))
         {
            serviceCert = ServiceBusHelper.ExtractNamespace(Description.Endpoints[0].Address.Uri);
         }
         ConfigureAnonymousMessageSecurity(location,storeName,X509FindType.FindBySubjectName,serviceCert);
      }
      public void ConfigureAnonymousMessageSecurity(StoreLocation location,StoreName storeName,X509FindType findType,object findValue)
      {
         Credentials.ServiceCertificate.SetCertificate(location,storeName,findType,findValue);
         Authorization.PrincipalPermissionMode = PrincipalPermissionMode.None;

         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {    
            ServiceBusHelper.ConfigureBinding(endpoint.Binding);
         } 
      }
      public void ConfigureMessageSecurity()
      {
         ConfigureMessageSecurity("",StoreLocation.LocalMachine,StoreName.My,true,null);
      }      
      public void ConfigureMessageSecurity(string serviceCert)
      {
         ConfigureMessageSecurity(serviceCert,StoreLocation.LocalMachine,StoreName.My,true,null);
      }
      public void ConfigureMessageSecurity(string serviceCert,string applicationName)
      {
         ConfigureMessageSecurity(serviceCert,StoreLocation.LocalMachine,StoreName.My,true,applicationName);
      }
      public void ConfigureMessageSecurity(string serviceCert,bool useProviders,string applicationName)
      {
         ConfigureMessageSecurity(serviceCert,StoreLocation.LocalMachine,StoreName.My,useProviders,applicationName);
      }
      public void ConfigureMessageSecurity(string serviceCert,StoreLocation location,StoreName storeName,bool useProviders,string applicationName)
      {
         if(serviceCert == String.Empty)
         {
            serviceCert = ServiceBusHelper.ExtractNamespace(Description.Endpoints[0].Address.Uri);
         }
         ConfigureMessageSecurity(location,storeName,X509FindType.FindBySubjectName,serviceCert,useProviders,applicationName);
      }
      public void ConfigureMessageSecurity(StoreLocation location,StoreName storeName,X509FindType findType,object findValue,bool useProviders,string applicationName)
      {
         Credentials.ServiceCertificate.SetCertificate(location,storeName,findType,findValue);

         foreach(ServiceEndpoint endpoint in Description.Endpoints)
         {    
            ServiceBusHelper.ConfigureBinding(endpoint.Binding,false);
         }
         if(useProviders)
         {
            Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseAspNetRoles;
            Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.MembershipProvider;

            SecurityBehavior.EnableRoleManager();

            string application;

            if(String.IsNullOrEmpty(applicationName))
            {
               applicationName = Membership.ApplicationName;
            }
            if(String.IsNullOrEmpty(applicationName) || applicationName == "/")
            {
               if(String.IsNullOrEmpty(Assembly.GetEntryAssembly().GetName().Name))
               {
                  application = AppDomain.CurrentDomain.FriendlyName;
               }
               else
               {
                  application = Assembly.GetEntryAssembly().GetName().Name;
               }
            }
            else
            {
               application = applicationName;
            }
            Membership.ApplicationName = application;
            Roles.ApplicationName = application;
         }
      }
   }
}
