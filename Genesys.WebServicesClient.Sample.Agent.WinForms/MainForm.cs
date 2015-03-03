using Genesys.WebServicesClient.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Sample.Agent.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            genesysDevice.BindControl("UserState.State", deviceStateLabel, "Text");
            genesysDevice.BindControl("UserState.DisplayName", deviceStateDisplayLabel, "Text");
            genesysCallManager.BindControl("ActiveCall.Id", activeCallIdLabel, "Text");
            genesysCallManager.BindControl("ActiveCall.State", activeCallStateLabel, "Text");
            genesysCallManager.BindControl("ActiveCall.AnswerCapable", answerButton, "Enabled");
            genesysCallManager.BindControl("ActiveCall.HangupCapable", hangupButton, "Enabled");
            genesysCallManager.BindControl("ActiveCall.InitiateTransferCapable", initiateTransferButton, "Enabled");
            genesysCallManager.BindControl("ActiveCall.CompleteTransferCapable", completeTransferButton, "Enabled");
            genesysCallManager.BindControl("ActiveCall.UpdateUserDataCapable", updateUserDataButton, "Enabled");

            callsDataGrid.DataSource = genesysCallManager.Calls;
            genesysUser.Updated += (s, e) => UpdateUserDataGrid();

            genesysUser.AvailableChanged += (s, e) => RefreshConnectionComponents();
            RefreshConnectionComponents();

            genesysUser.Updated += (s, e) => ObtainOption();
        }

        void RefreshConnectionComponents()
        {
            usernameTextBox.Enabled = !genesysUser.Available;
            passwordTextBox.Enabled = !genesysUser.Available;
            connectButton.Enabled = !genesysUser.Available;
            disconnectButton.Enabled = genesysUser.Available;
        }

        void UpdateUserDataGrid()
        {
            if (genesysCallManager.ActiveCall != null)
            {
                userDataGrid.SelectedObject = null;
                userDataGrid.SelectedObject = genesysCallManager.ActiveCall.UserData;
                userDataGrid.Refresh();
            }
        }

        void ObtainOption()
        {
            IDictionary<string, object> section;
            if (genesysUser.Settings.TryGetValue("interaction-workspace", out section))
            {
                object result;
                if (section.TryGetValue("voice.mark-done-on-release", out result))
                {
                    optionLabel.Text = result.ToString();
                }
            }
        }

        public void DisplayError(string error)
        {
            errorsLabel.Text = error;
        }

        async void connectButton_Click(object sender, EventArgs e)
        {
            genesysConnection.Username = usernameTextBox.Text;
            genesysConnection.Password = passwordTextBox.Text;

            try
            {
                await genesysConnection.StartAsync();
                await genesysUser.StartAsync();
            }
            catch (Exception)
            {
                genesysConnection.Stop();
                throw;
            }
        }

        void disconnectButton_Click(object sender, EventArgs e)
        {
            genesysConnection.Dispose();
        }

        async void readyButton_Click(object sender, EventArgs e)
        {
            await genesysUser.ChangeState("Ready");
        }

        async void notReadyButton_Click(object sender, EventArgs e)
        {
            await genesysUser.ChangeState("NotReady");
        }

        void answerButton_Click(object sender, EventArgs e)
        {
            genesysCallManager.ActiveCall.Answer();
        }

        void hangupButton_Click(object sender, EventArgs e)
        {
            genesysCallManager.ActiveCall.Hangup();
        }

        void initiateTransferButton_Click(object sender, EventArgs e)
        {
            genesysCallManager.ActiveCall.InitiateTransfer(phoneNumberTextBox.Text);
        }

        void completeTransferButton_Click(object sender, EventArgs e)
        {
            genesysCallManager.ActiveCall.CompleteTransfer();
        }

        void updateUserDataButton_Click(object sender, EventArgs e)
        {
            genesysCallManager.ActiveCall.UpdateUserData(new Dictionary<string, object> {
                { userDataKeyTextBox.Text, userDataValueTextBox.Text }
            });
        }
    }
}
