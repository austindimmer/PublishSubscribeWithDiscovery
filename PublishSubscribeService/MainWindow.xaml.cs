using ServiceLibrary.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        ServiceHost _PubSubDiscoveryHost;
        public MainWindow()
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
            _PubSubDiscoveryHost.Open();
        }
    }
}
