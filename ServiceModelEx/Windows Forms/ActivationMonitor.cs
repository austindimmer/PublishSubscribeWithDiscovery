// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.Windows.Forms;

namespace ServiceModelEx
{
   [ServiceContract]
   interface IActivationMonitor
   {
      [OperationContract]
      void ActivateApplication();
   }
   class ActivationMonitorService : IActivationMonitor 
   {
      public void ActivateApplication()
      {
         Form form = SingletonApp.MainForm;
         if(form != null)
         {
            if(!form.IsDisposed)
            {
               //This executes on the thread from the thread pool. Need to marshal to the form
               //Use anonymous method to wrap WindowState property
               if(form.WindowState == FormWindowState.Minimized)
               {
                  Action restore = delegate()
                                   {
                                      form.WindowState = FormWindowState.Normal;
                                   };
                  form.Invoke(restore,new object[]{});
               }
               Action activate = form.Activate;
               form.Invoke(activate,new object[]{});
            }
         } 
      }
   }
}
