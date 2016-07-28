using ServiceLibrary;
using ServiceLibrary.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
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

namespace Subscriber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SubscribeMainWindow : Window
    {
        ServiceHost<SubscriptionService> _SubscriptionHost;

        public SubscribeMainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            ConfigureSubscriber();
        }



        private void ConfigureSubscriber()
        {
            FaultHandledOperations.ExecuteFaultHandledOperation(() =>
            {

                _SubscriptionHost = DiscoveryPublishService<IMyEvents>.CreateHost<SubscriptionService>();

                //Configure Host

                _SubscriptionHost.Closed += _SubscriptionHost_Closed;
                _SubscriptionHost.Faulted += _SubscriptionHost_Faulted;
                _SubscriptionHost.Closed += _SubscriptionHost_Closed1;
                _SubscriptionHost.Open();

            });

        }

        private void _SubscriptionHost_Closed1(object sender, EventArgs e)
        {
            Debug.WriteLine("_SubscriptionHost_Closed1");
        }

        private void _SubscriptionHost_Faulted(object sender, EventArgs e)
        {
            Debug.WriteLine("_SubscriptionHost_Faulted");
        }

        private void _SubscriptionHost_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("_SubscriptionHost_Closed");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _SubscriptionHost.Close();
        }
    }
}
