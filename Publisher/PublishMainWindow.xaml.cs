using ServiceLibrary;
using ServiceLibrary.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

namespace Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PublishMainWindow : Window
    {
        IMyEvents _Proxy;

        public PublishMainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;

            var interval = TimeSpan.FromSeconds(5);
            var subcription = Observable.Interval(interval)            .Do(_ =>
            {
                PublishEvents();
            }).Subscribe();

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //(_Proxy as ICommunicationObject).Close();
        }

        private void PublishEvents()
        {
            FaultHandledOperations.ExecuteFaultHandledOperation(() =>
            {
                _Proxy = DiscoveryPublishService<IMyEvents>.CreateChannel();
                //_Proxy.OnEvent1();
                //_Proxy.OnEvent2(1);
                _Proxy.OnEvent3(2, "Hello");
                (_Proxy as ICommunicationObject).Close();

            });

        }
    }
}
