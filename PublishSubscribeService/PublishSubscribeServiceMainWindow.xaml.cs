using ServiceLibrary.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PublishSubscribeService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PublishSubscribeServiceMainWindow : Window
    {
        ServiceHost _PubSubDiscoveryHost;
        public PublishSubscribeServiceMainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            ConfigurePubSubDiscoveryService();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _PubSubDiscoveryHost.Close();
        }

        private void ConfigurePubSubDiscoveryService()
        {
            _PubSubDiscoveryHost = DiscoveryPublishService<IMyEvents>.CreateHost<MyPublishService>();
            _PubSubDiscoveryHost.Closed += _PubSubDiscoveryHost_Closed;
            _PubSubDiscoveryHost.Faulted += _PubSubDiscoveryHost_Faulted;
            _PubSubDiscoveryHost.Opened += _PubSubDiscoveryHost_Opened;
            _PubSubDiscoveryHost.Open();
        }

        private void _PubSubDiscoveryHost_Opened(object sender, EventArgs e)
        {
            Debug.WriteLine("_PubSubDiscoveryHost_Opened");
        }

        private void _PubSubDiscoveryHost_Faulted(object sender, EventArgs e)
        {
            Debug.WriteLine("_PubSubDiscoveryHost_Faulted");
        }

        private void _PubSubDiscoveryHost_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("_PubSubDiscoveryHost_Closed");
        }
    }
}
