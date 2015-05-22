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

            genesysConnection = new GenesysConnection()
                {
                    WebSocketsEnabled = false,
                };

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
            UserPanel.DataContext = genesysUser;
            DevicePanel.DataContext = genesysDevice;
            CallsPanel.DataContext = genesysCallManager;
            ActiveCallPanel.DataContext = genesysCallManager;
            CallDataGrid.ItemsSource = genesysCallManager.Calls;

            genesysUser.Updated += (s, e) =>
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

        async void OpenConnection_Click(object sender, RoutedEventArgs e)
        {
            genesysConnection.ServerUri = ServerUri.Text;
            genesysConnection.UserName = Username.Text;
            genesysConnection.Password = Password.Text;

            try
            {
                await genesysConnection.StartAsync();
                await genesysUser.StartAsync();
            }
            catch (Exception ex)
            {
                genesysConnection.Stop();

                // OperationCanceledException is thrown if Close was requested while Opening. That's OK
                if (!(ex is OperationCanceledException))
                    throw;
            }
        }

        void CloseConnection_Click(object sender, RoutedEventArgs e)
        {
            genesysConnection.Dispose();
        }

        void SetReady_Click(object sender, RoutedEventArgs e)
        {
            genesysUser.DoOperation("Ready");
        }

        void SetNotReady_Click(object sender, RoutedEventArgs e)
        {
            genesysUser.DoOperation("NotReady");
        }

        void StartSessionChat_Click(object sender, RoutedEventArgs e)
        {
            genesysUser.StartContactCenterSession(new string[] { "chat" }, null, null, null, null);
        }
        
        void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.Answer();
        }

        void HangupButton_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.Hangup();
        }

        void AttachUserData_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.AttachUserData(new Dictionary<string, string>()
                {
                    { UserDataKey.Text, UserDataValue.Text },
                });
        }

        void UpdateUserData_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.UpdateUserData(new Dictionary<string, string>()
                {
                    { UserDataKey.Text, UserDataValue.Text },
                });
        }

        void TestToast_Click(object sender, RoutedEventArgs e)
        {
            ToastWindow toast = new ToastWindow();
            toast.Show();
        }
    }
}
