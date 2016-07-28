// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace ServiceModelEx
{
   [AttributeUsage(AttributeTargets.Class)]
   public class SecurityBehaviorAttribute : Attribute,IServiceBehavior
   {
      SecurityBehavior m_SecurityBehavior;
      string m_ApplicationName;

      /// <summary>
      /// </summary>
      /// <param name="mode">If set to ServiceSecurity.Anonymous,ServiceSecurity.BusinessToBusiness or ServiceSecurity.Internet then the service certificate must be listed in config file</param>
      public SecurityBehaviorAttribute(ServiceSecurity mode)
      {
         m_SecurityBehavior = new SecurityBehavior(mode);
      }
      /// <summary>
      /// </summary>
      /// <param name="mode">Certificate is looked up by name from LocalMachine/My store</param>
      public SecurityBehaviorAttribute(ServiceSecurity mode,string serviceCertificateName)
      {
         m_SecurityBehavior = new SecurityBehavior(mode,serviceCertificateName);
      }

      public SecurityBehaviorAttribute(ServiceSecurity mode,StoreLocation storeLocation,StoreName storeName,X509FindType findType,string serviceCertificateName)
      {
         m_SecurityBehavior = new SecurityBehavior(mode,storeLocation,storeName,findType,serviceCertificateName);
      }
      public bool ImpersonateAll
      {
         get;set;
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
         get;set;
      }

      public bool SecurityAuditEnabled
      {
         get;set;
      }


      void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase serviceHostBase,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
      {
         m_SecurityBehavior.AddBindingParameters(description,serviceHostBase,endpoints,parameters);
      }
      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase serviceHostBase)
      {}
      void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase serviceHostBase)
      {
         m_SecurityBehavior.UseAspNetProviders = UseAspNetProviders;
         m_SecurityBehavior.ApplicationName = ApplicationName;
         m_SecurityBehavior.ImpersonateAll = ImpersonateAll;

         m_SecurityBehavior.Validate(description,serviceHostBase);

         if(SecurityAuditEnabled)
         {
            ServiceSecurityAuditBehavior securityAudit = serviceHostBase.Description.Behaviors.Find<ServiceSecurityAuditBehavior>();
            if(securityAudit == null)
            {
               securityAudit = new ServiceSecurityAuditBehavior();
               securityAudit.AuditLogLocation = AuditLogLocation.Application;
               securityAudit.MessageAuthenticationAuditLevel = AuditLevel.SuccessOrFailure;
               securityAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;
               serviceHostBase.Description.Behaviors.Add(securityAudit);
            }
         }
      }
   }
}




