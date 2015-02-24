using Genesys.WebServicesClient.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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
    public partial class MainWindow : Window
    {
        readonly GenesysConnection genesysConnection;
        readonly GenesysUser genesysUser;
        readonly GenesysDevice genesysDevice;
        readonly GenesysCallManager genesysCallManager;

        public MainWindow()
        {
            InitializeComponent();

            genesysConnection = new GenesysConnection();

            genesysUser = new GenesysUser()
                {
                    Connection = genesysConnection,
                };

            genesysDevice = new GenesysDevice()
                {
                    User = genesysUser,
                };

            genesysCallManager = new GenesysCallManager()
                {
                    User = genesysUser,
                };

            ConnectionPanel.DataContext = genesysConnection;
            DevicePanel.DataContext = genesysDevice;
            CallsPanel.DataContext = genesysCallManager;
            ActiveCallPanel.DataContext = genesysCallManager;
            CallDataGrid.ItemsSource = genesysCallManager.Calls;

            genesysUser.ResourceUpdated += (s, e) =>
            {
                UpdateUserDataGrid();
            };

            genesysCallManager.Calls.ListChanged += Calls_ListChanged;
        }

        void Calls_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                new ToastWindow(genesysCallManager.Calls[e.NewIndex]).Show();
            }
        }

        void UpdateUserDataGrid()
        {
            if (genesysCallManager.ActiveCall != null)
            {
                userDataPropertyGrid.SelectedObject = null;
                userDataPropertyGrid.SelectedObject = genesysCallManager.ActiveCall.UserData;
                userDataPropertyGrid.Refresh();
            }
        }

        void SetReady_Click(object sender, RoutedEventArgs e)
        {
            genesysUser.ChangeState("Ready");
        }

        void SetNotReady_Click(object sender, RoutedEventArgs e)
        {
            genesysUser.ChangeState("NotReady");
        }

        void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.Answer();
        }

        void HangupButton_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.Hangup();
        }

        void TestToast_Click(object sender, RoutedEventArgs e)
        {
            ToastWindow toast = new ToastWindow();
            toast.Show();
        }

        async void OpenConnection_Click(object sender, RoutedEventArgs e)
        {
            genesysConnection.ServerUri = ServerUri.Text;
            genesysConnection.Username = Username.Text;
            genesysConnection.Password = Password.Text;

            try
            {
                await genesysConnection.OpenAsync();
                await genesysUser.ActivateAsync();
            }
            catch (Exception)
            {
                genesysConnection.Close();
                throw;
            }
        }

        void CloseConnection_Click(object sender, RoutedEventArgs e)
        {
            genesysConnection.Dispose();
        }
    }
}
