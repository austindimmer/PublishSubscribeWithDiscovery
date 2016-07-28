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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var interval = TimeSpan.FromSeconds(5);
            var subcription = Observable.Interval(interval)            .Do(_ =>
            {
                PublishEvents();
            }).Subscribe();

        }

        private void PublishEvents()
        {
            IMyEvents proxy = DiscoveryPublishService<IMyEvents>.CreateChannel();
            proxy.OnEvent1();
            proxy.OnEvent2(1);
            proxy.OnEvent3(2, "Hello");
            (proxy as ICommunicationObject).Close();
        }
    }
}
