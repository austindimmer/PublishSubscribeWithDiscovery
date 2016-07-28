// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace ServiceModelEx
{
   public class NetNamedPipeContextBinding : NetNamedPipeBinding
   {
      internal const string SectionName = "netNamedPipeContextBinding";

      public ProtectionLevel ContextProtectionLevel
      {get;set;}

      public NetNamedPipeContextBinding()
      {
         ContextProtectionLevel = ProtectionLevel.EncryptAndSign;
      }
      public NetNamedPipeContextBinding(NetNamedPipeSecurityMode securityMode) : base(securityMode)
      {
         ContextProtectionLevel = ProtectionLevel.EncryptAndSign;
      }
      public NetNamedPipeContextBinding(string configurationName)
      {
         ContextProtectionLevel = ProtectionLevel.EncryptAndSign;
         ApplyConfiguration(configurationName);
      }
      //The heart of the custom binding 
      public override BindingElementCollection CreateBindingElements()
      {
         BindingElement element = new ContextBindingElement(ContextProtectionLevel,ContextExchangeMechanism.ContextSoapHeader);

         BindingElementCollection elements = base.CreateBindingElements();
         elements.Insert(0,element);

         return elements;
      }
      void ApplyConfiguration(string configurationName)
      {
         Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ServiceModelSectionGroup sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);

         BindingsSection bindings = sectionGroup.Bindings;

         NetNamedPipeContextBindingCollectionElement section = (NetNamedPipeContextBindingCollectionElement)bindings[SectionName];
         NetNamedPipeContextBindingElement element = section.Bindings[configurationName];
         if(element == null)
         {
            throw new ConfigurationErrorsException("There is no binding named " + configurationName + " at " + section.BindingName);
         }
         else
         {
            element.ApplyConfiguration(this);
         }
      }
   }
   public class NetNamedPipeContextBindingElement : NetNamedPipeBindingElement
   {
      const string ContextProtectionLevelName = "contextProtectionLevel";

      public NetNamedPipeContextBindingElement()
      {
         Initialize();
      }
      public NetNamedPipeContextBindingElement(string name) : base(name)
      {
         Initialize();
      }

      void Initialize()
      {
         ConfigurationProperty property = new ConfigurationProperty(ContextProtectionLevelName,typeof(ProtectionLevel),ProtectionLevel.EncryptAndSign);
         Properties.Add(property);

         ContextProtectionLevel = ProtectionLevel.EncryptAndSign;
      }
      protected override void OnApplyConfiguration(Binding binding)
      {
         base.OnApplyConfiguration(binding);
         NetNamedPipeContextBinding netNamedPipeContextBinding = binding as NetNamedPipeContextBinding;
         Debug.Assert(netNamedPipeContextBinding != null);

         netNamedPipeContextBinding.ContextProtectionLevel = ContextProtectionLevel;
      }
      protected override Type BindingElementType
      {
         get
         {
            return typeof(NetNamedPipeContextBinding);
         }
      }

      public ProtectionLevel ContextProtectionLevel
      {
         get
         {
            return (ProtectionLevel)base[ContextProtectionLevelName];
         }
         set
         {
            base[ContextProtectionLevelName] = value;
         }
      }
   }
   public class NetNamedPipeContextBindingCollectionElement : StandardBindingCollectionElement<NetNamedPipeContextBinding,NetNamedPipeContextBindingElement>
   {}
}
