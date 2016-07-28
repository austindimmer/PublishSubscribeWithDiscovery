// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;

namespace ServiceModelEx
{
   public static class SingletonApp 
   {
      static Mutex m_Mutex;
      static Form m_MainForm;
      static ServiceHost m_Host;

      public static void Run()
      {
         if(IsFirstInstance())
         {
            Application.ApplicationExit += OnExit;
            HostActivationMonitor();
            Application.Run();
         }
         else
         {
            ActivateFirstInstance();
         }
      }
      public static void Run(ApplicationContext context)
      {
         if(IsFirstInstance())
         {
            MainForm = context.MainForm;
            Application.ApplicationExit += OnExit;
            HostActivationMonitor();
            Application.Run(context);
         }
         else
         {
            ActivateFirstInstance();
         }
      }
      public static void Run(Form mainForm)
      {
         if(IsFirstInstance())
         {
            Application.ApplicationExit += OnExit;
            MainForm = mainForm;
            HostActivationMonitor();
            Application.Run(mainForm);
         }
         else
         {
            ActivateFirstInstance();
         }
      }
      static bool IsFirstInstance()
      {
         Assembly assembly = Assembly.GetEntryAssembly();
         string name = assembly.FullName;

         m_Mutex = new Mutex(false,name);
         bool owned = false;
         owned = m_Mutex.WaitOne(TimeSpan.Zero,false);
         return owned;
      }
      static void OnExit(object sender,EventArgs args)
      {
         m_Host.Close();
         m_Mutex.ReleaseMutex();
         m_Mutex.Close();
      }
      static void HostActivationMonitor()
      {
         NetNamedPipeBinding binding = new NetNamedPipeBinding();

         m_Host = new ServiceHost<ActivationMonitorService>();
         m_Host.AddServiceEndpoint(typeof(IActivationMonitor),binding,MonitorServiceAddress);
         m_Host.Open();
      }
      static internal Form MainForm
      {
         get
         {
            return m_MainForm;
         }
         set
         {
            m_MainForm = value;
         }
      }
      static void ActivateFirstInstance()
      {
         NetNamedPipeBinding binding = new NetNamedPipeBinding();
         IActivationMonitor monitor = ChannelFactory<IActivationMonitor>.CreateChannel(binding,new EndpointAddress(MonitorServiceAddress));
         monitor.ActivateApplication();
         ICommunicationObject proxy = monitor as ICommunicationObject;
         proxy.Close();
      }
      static string MonitorServiceAddress
      {
         get
         {
            Assembly assembly = Assembly.GetEntryAssembly();
            string pipeName = assembly.FullName;
            return "net.pipe://localhost/" + pipeName;
         }
      }
   }
}
