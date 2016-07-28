// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Configuration;

namespace ServiceModelEx.Properties
{  
   internal sealed partial class Settings
   {      
      public Settings()
      {}      
      void SettingChangingEventHandler(object sender,SettingChangingEventArgs e)
      {}      
      void SettingsSavingEventHandler(object sender,System.ComponentModel.CancelEventArgs e)
      {
         // Add code to handle the SettingsSaving event here.
      }
   }
}
