using Genesys.WebServicesClient.Components;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Genesys.WebServicesClient.Sample.Agent.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly GenesysConnection genesysConnection = new GenesysConnection();
        readonly GenesysAgent genesysAgent = new GenesysAgent();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = genesysAgent;
        }

        void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            genesysAgent.State = "Ready";
        }

        void NotReadyButton_Click(object sender, RoutedEventArgs e)
        {
            genesysAgent.State = "NotReady2";
        }
        
    }
}
