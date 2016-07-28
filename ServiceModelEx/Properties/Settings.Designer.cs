// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ServiceModelEx.Properties
{
   [CompilerGenerated]
   [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator","14.0.0.0")]
   internal sealed partial class Settings : ApplicationSettingsBase
   {     
      static Settings defaultInstance = ((Settings)(ApplicationSettingsBase.Synchronized(new Settings())));
      public static Settings Default
      {
         get
         {
            return defaultInstance;
         }
      }
      
      [ApplicationScopedSetting]
      [DebuggerNonUserCode]
      [SpecialSetting(SpecialSetting.ConnectionString)]
      [DefaultSettingValue("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True")]
      public string PublishSubscribeConnectionString
      {
         get
         {
            return ((string)(this["PublishSubscribeConnectionString"]));
         }
      }
      
      [ApplicationScopedSetting]
      [DebuggerNonUserCode]
      [SpecialSetting(SpecialSetting.ConnectionString)]
      [DefaultSettingValue("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True")]
      public string WCFLogbookConnectionString
      {
         get
         {
            return ((string)(this["WCFLogbookConnectionString"]));
         }
      }
   }
}
