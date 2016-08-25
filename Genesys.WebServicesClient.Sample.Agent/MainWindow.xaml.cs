using Genesys.WebServicesClient.Components;
using Genesys.WebServicesClient.Sample.Agent.WPF.Properties;
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
        readonly GenesysInteractionManager genesysInteractionManager;
        readonly GenesysChannelManager genesysChannelManager;

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

            genesysInteractionManager = new GenesysInteractionManager()
                {
                    User = genesysUser,
                };

            genesysChannelManager = new GenesysChannelManager()
                {
                    User = genesysUser,
                };

            ConnectionPanel.DataContext = genesysConnection;
            UserPanel.DataContext = genesysUser;
            DevicePanel.DataContext = genesysDevice;
            CallsPanel.DataContext = genesysInteractionManager;
            ActiveCallPanel.DataContext = genesysInteractionManager;
            CallDataGrid.ItemsSource = genesysInteractionManager.Calls;
            ChannelsPanel.DataContext = genesysChannelManager;
            ChatDataGrid.ItemsSource = genesysInteractionManager.Chats;
            ActiveChatPanel.DataContext = genesysInteractionManager;

            genesysUser.Updated += (s, e) =>
            {
                UpdateUserDataGrid();
            };

            genesysInteractionManager.Calls.ListChanged += Calls_ListChanged;

            genesysInteractionManager.Chats.ListChanged += Chats_ListChanged;
        }

        void Calls_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                new ToastWindow(genesysInteractionManager.Calls[e.NewIndex]).Show();
            }
        }

        void Chats_ListChanged(object sender, ListChangedEventArgs e)
        {
            ChatMessagesDataGrid.ItemsSource = genesysInteractionManager.ActiveChatMessages;
        }

        void UpdateUserDataGrid()
        {
            if (genesysInteractionManager.ActiveCall != null)
            {
                userDataPropertyGrid.SelectedObject = null;
                userDataPropertyGrid.SelectedObject = genesysInteractionManager.ActiveCall.UserData;
                userDataPropertyGrid.Refresh();
            }
        }

        async void OpenConnection_Click(object sender, RoutedEventArgs e)
        {
            genesysConnection.ServerUri = ServerUri.Text;
            genesysConnection.UserName = Username.Text;
            genesysConnection.Password = Password.Text;
            genesysConnection.OpenTimeoutMs = Settings.Default.OpenConnectionTimeoutMs;

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
            genesysInteractionManager.ActiveCall.Answer();
        }

        void HangupButton_Click(object sender, RoutedEventArgs e)
        {
            genesysInteractionManager.ActiveCall.Hangup();
        }

        void AttachUserData_Click(object sender, RoutedEventArgs e)
        {
            genesysInteractionManager.ActiveCall.AttachUserData(new Dictionary<string, string>()
                {
                    { UserDataKey.Text, UserDataValue.Text },
                });
        }

        void UpdateUserData_Click(object sender, RoutedEventArgs e)
        {
            genesysInteractionManager.ActiveCall.UpdateUserData(new Dictionary<string, string>()
                {
                    { UserDataKey.Text, UserDataValue.Text },
                });
        }

        void TestToast_Click(object sender, RoutedEventArgs e)
        {
            ToastWindow toast = new ToastWindow();
            toast.Show();
        }

        void StartSession_Click(object sender, RoutedEventArgs e)
        {
            var channels = new List<string>();

            if (ChatChannelCheckBox.IsChecked.Value)
                channels.Add("chat");

            if (TwitterChannelCheckBox.IsChecked.Value)
                channels.Add("twitter");

            genesysUser.StartContactCenterSession(channels);
        }

        void EndSession_Click(object sender, RoutedEventArgs e)
        {
            genesysUser.EndContactCenterSession();
        }

        void AcceptChat_Click(object sender, RoutedEventArgs e)
        {
           genesysInteractionManager.ActiveChat.Accept(Username.Text);
        }

        void RejectChat_Click(object sender, RoutedEventArgs e)
        {
            genesysInteractionManager.ActiveChat.Reject();
        }

        void CompleteChat_Click(object sender, RoutedEventArgs e)
        {
            genesysInteractionManager.ActiveChat.Complete();
        }

        void SendMessageChat_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        void SendMessage()
        {
            genesysInteractionManager.ActiveChat.SendMessage(ChatMessageTextBox.Text);
            ChatMessageTextBox.Clear();
        }

        void LeaveChat_Click(object sender, RoutedEventArgs e)
        {
            genesysInteractionManager.ActiveChat.Leave();
        }
    }
}
