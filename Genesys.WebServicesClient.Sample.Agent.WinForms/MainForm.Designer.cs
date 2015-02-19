namespace Genesys.WebServicesClient.Sample.Agent.WinForms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.readyButton = new System.Windows.Forms.Button();
            this.notReadyButton = new System.Windows.Forms.Button();
            this.errorsLabel = new System.Windows.Forms.Label();
            this.activeCallStateLabel = new System.Windows.Forms.Label();
            this.callsDataGrid = new System.Windows.Forms.DataGridView();
            this.answerButton = new System.Windows.Forms.Button();
            this.hangupButton = new System.Windows.Forms.Button();
            this.initiateTransferButton = new System.Windows.Forms.Button();
            this.completeTransferButton = new System.Windows.Forms.Button();
            this.phoneNumberTextBox = new System.Windows.Forms.TextBox();
            this.activeCallIdLabel = new System.Windows.Forms.Label();
            this.deviceStateLabel = new System.Windows.Forms.Label();
            this.connectButton = new System.Windows.Forms.Button();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.deviceStateDisplayLabel = new System.Windows.Forms.Label();
            this.userGroupBox = new System.Windows.Forms.GroupBox();
            this.deviceGroupBox = new System.Windows.Forms.GroupBox();
            this.connectionGroupBox = new System.Windows.Forms.GroupBox();
            this.callsGroupBox = new System.Windows.Forms.GroupBox();
            this.userDataValueTextBox = new System.Windows.Forms.TextBox();
            this.updateUserDataButton = new System.Windows.Forms.Button();
            this.userDataKeyTextBox = new System.Windows.Forms.TextBox();
            this.userDataGrid = new System.Windows.Forms.PropertyGrid();
            this.optionLabel = new System.Windows.Forms.Label();
            this.genesysCallManager = new Genesys.WebServicesClient.Components.GenesysCallManager();
            this.genesysDevice = new Genesys.WebServicesClient.Components.GenesysDevice();
            this.genesysConnection = new Genesys.WebServicesClient.Components.GenesysConnection(this.components);
            this.genesysUser = new Genesys.WebServicesClient.Components.GenesysUser();
            ((System.ComponentModel.ISupportInitialize)(this.callsDataGrid)).BeginInit();
            this.userGroupBox.SuspendLayout();
            this.deviceGroupBox.SuspendLayout();
            this.connectionGroupBox.SuspendLayout();
            this.callsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // readyButton
            // 
            this.readyButton.Location = new System.Drawing.Point(6, 15);
            this.readyButton.Name = "readyButton";
            this.readyButton.Size = new System.Drawing.Size(75, 23);
            this.readyButton.TabIndex = 10;
            this.readyButton.Text = "Ready";
            this.readyButton.UseVisualStyleBackColor = true;
            this.readyButton.Click += new System.EventHandler(this.readyButton_Click);
            // 
            // notReadyButton
            // 
            this.notReadyButton.Location = new System.Drawing.Point(6, 45);
            this.notReadyButton.Name = "notReadyButton";
            this.notReadyButton.Size = new System.Drawing.Size(75, 23);
            this.notReadyButton.TabIndex = 11;
            this.notReadyButton.Text = "Not Ready";
            this.notReadyButton.UseVisualStyleBackColor = true;
            this.notReadyButton.Click += new System.EventHandler(this.notReadyButton_Click);
            // 
            // errorsLabel
            // 
            this.errorsLabel.AutoSize = true;
            this.errorsLabel.ForeColor = System.Drawing.Color.DarkRed;
            this.errorsLabel.Location = new System.Drawing.Point(18, 361);
            this.errorsLabel.Name = "errorsLabel";
            this.errorsLabel.Size = new System.Drawing.Size(40, 13);
            this.errorsLabel.TabIndex = 3;
            this.errorsLabel.Text = "[Errors]";
            // 
            // activeCallStateLabel
            // 
            this.activeCallStateLabel.AutoSize = true;
            this.activeCallStateLabel.Location = new System.Drawing.Point(13, 17);
            this.activeCallStateLabel.Name = "activeCallStateLabel";
            this.activeCallStateLabel.Size = new System.Drawing.Size(88, 13);
            this.activeCallStateLabel.TabIndex = 0;
            this.activeCallStateLabel.Text = "[Active call state]";
            // 
            // callsDataGrid
            // 
            this.callsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.callsDataGrid.Location = new System.Drawing.Point(16, 154);
            this.callsDataGrid.Name = "callsDataGrid";
            this.callsDataGrid.Size = new System.Drawing.Size(240, 150);
            this.callsDataGrid.TabIndex = 4;
            // 
            // answerButton
            // 
            this.answerButton.Location = new System.Drawing.Point(17, 58);
            this.answerButton.Name = "answerButton";
            this.answerButton.Size = new System.Drawing.Size(75, 23);
            this.answerButton.TabIndex = 5;
            this.answerButton.Text = "Answer";
            this.answerButton.UseVisualStyleBackColor = true;
            this.answerButton.Click += new System.EventHandler(this.answerButton_Click);
            // 
            // hangupButton
            // 
            this.hangupButton.Location = new System.Drawing.Point(99, 58);
            this.hangupButton.Name = "hangupButton";
            this.hangupButton.Size = new System.Drawing.Size(75, 23);
            this.hangupButton.TabIndex = 6;
            this.hangupButton.Text = "Hang up";
            this.hangupButton.UseVisualStyleBackColor = true;
            this.hangupButton.Click += new System.EventHandler(this.hangupButton_Click);
            // 
            // initiateTransferButton
            // 
            this.initiateTransferButton.Location = new System.Drawing.Point(17, 88);
            this.initiateTransferButton.Name = "initiateTransferButton";
            this.initiateTransferButton.Size = new System.Drawing.Size(110, 23);
            this.initiateTransferButton.TabIndex = 7;
            this.initiateTransferButton.Text = "Initiate transfer";
            this.initiateTransferButton.UseVisualStyleBackColor = true;
            this.initiateTransferButton.Click += new System.EventHandler(this.initiateTransferButton_Click);
            // 
            // completeTransferButton
            // 
            this.completeTransferButton.Location = new System.Drawing.Point(17, 118);
            this.completeTransferButton.Name = "completeTransferButton";
            this.completeTransferButton.Size = new System.Drawing.Size(110, 23);
            this.completeTransferButton.TabIndex = 8;
            this.completeTransferButton.Text = "Complete Transfer";
            this.completeTransferButton.UseVisualStyleBackColor = true;
            this.completeTransferButton.Click += new System.EventHandler(this.completeTransferButton_Click);
            // 
            // phoneNumberTextBox
            // 
            this.phoneNumberTextBox.Location = new System.Drawing.Point(133, 90);
            this.phoneNumberTextBox.Name = "phoneNumberTextBox";
            this.phoneNumberTextBox.Size = new System.Drawing.Size(75, 20);
            this.phoneNumberTextBox.TabIndex = 9;
            // 
            // activeCallIdLabel
            // 
            this.activeCallIdLabel.AutoSize = true;
            this.activeCallIdLabel.Location = new System.Drawing.Point(13, 33);
            this.activeCallIdLabel.Name = "activeCallIdLabel";
            this.activeCallIdLabel.Size = new System.Drawing.Size(73, 13);
            this.activeCallIdLabel.TabIndex = 10;
            this.activeCallIdLabel.Text = "[Active call id]";
            // 
            // deviceStateLabel
            // 
            this.deviceStateLabel.AutoSize = true;
            this.deviceStateLabel.Location = new System.Drawing.Point(8, 16);
            this.deviceStateLabel.Name = "deviceStateLabel";
            this.deviceStateLabel.Size = new System.Drawing.Size(38, 13);
            this.deviceStateLabel.TabIndex = 13;
            this.deviceStateLabel.Text = "[State]";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(6, 71);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 3;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(6, 19);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(126, 20);
            this.usernameTextBox.TabIndex = 1;
            this.usernameTextBox.Text = "paveld@redwings.com";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(6, 45);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(126, 20);
            this.passwordTextBox.TabIndex = 2;
            this.passwordTextBox.Text = "password";
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(6, 100);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectButton.TabIndex = 4;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // deviceStateDisplayLabel
            // 
            this.deviceStateDisplayLabel.AutoSize = true;
            this.deviceStateDisplayLabel.Location = new System.Drawing.Point(8, 35);
            this.deviceStateDisplayLabel.Name = "deviceStateDisplayLabel";
            this.deviceStateDisplayLabel.Size = new System.Drawing.Size(67, 13);
            this.deviceStateDisplayLabel.TabIndex = 13;
            this.deviceStateDisplayLabel.Text = "[State name]";
            // 
            // userGroupBox
            // 
            this.userGroupBox.Controls.Add(this.readyButton);
            this.userGroupBox.Controls.Add(this.notReadyButton);
            this.userGroupBox.Location = new System.Drawing.Point(12, 149);
            this.userGroupBox.Name = "userGroupBox";
            this.userGroupBox.Size = new System.Drawing.Size(138, 80);
            this.userGroupBox.TabIndex = 17;
            this.userGroupBox.TabStop = false;
            this.userGroupBox.Text = "User";
            // 
            // deviceGroupBox
            // 
            this.deviceGroupBox.Controls.Add(this.deviceStateLabel);
            this.deviceGroupBox.Controls.Add(this.deviceStateDisplayLabel);
            this.deviceGroupBox.Location = new System.Drawing.Point(12, 235);
            this.deviceGroupBox.Name = "deviceGroupBox";
            this.deviceGroupBox.Size = new System.Drawing.Size(138, 63);
            this.deviceGroupBox.TabIndex = 18;
            this.deviceGroupBox.TabStop = false;
            this.deviceGroupBox.Text = "Device";
            // 
            // connectionGroupBox
            // 
            this.connectionGroupBox.Controls.Add(this.usernameTextBox);
            this.connectionGroupBox.Controls.Add(this.connectButton);
            this.connectionGroupBox.Controls.Add(this.disconnectButton);
            this.connectionGroupBox.Controls.Add(this.passwordTextBox);
            this.connectionGroupBox.Location = new System.Drawing.Point(12, 12);
            this.connectionGroupBox.Name = "connectionGroupBox";
            this.connectionGroupBox.Size = new System.Drawing.Size(138, 131);
            this.connectionGroupBox.TabIndex = 19;
            this.connectionGroupBox.TabStop = false;
            this.connectionGroupBox.Text = "Connection";
            // 
            // callsGroupBox
            // 
            this.callsGroupBox.Controls.Add(this.userDataValueTextBox);
            this.callsGroupBox.Controls.Add(this.updateUserDataButton);
            this.callsGroupBox.Controls.Add(this.userDataKeyTextBox);
            this.callsGroupBox.Controls.Add(this.userDataGrid);
            this.callsGroupBox.Controls.Add(this.activeCallIdLabel);
            this.callsGroupBox.Controls.Add(this.phoneNumberTextBox);
            this.callsGroupBox.Controls.Add(this.completeTransferButton);
            this.callsGroupBox.Controls.Add(this.initiateTransferButton);
            this.callsGroupBox.Controls.Add(this.hangupButton);
            this.callsGroupBox.Controls.Add(this.answerButton);
            this.callsGroupBox.Controls.Add(this.callsDataGrid);
            this.callsGroupBox.Controls.Add(this.activeCallStateLabel);
            this.callsGroupBox.Location = new System.Drawing.Point(161, 12);
            this.callsGroupBox.Name = "callsGroupBox";
            this.callsGroupBox.Size = new System.Drawing.Size(519, 331);
            this.callsGroupBox.TabIndex = 20;
            this.callsGroupBox.TabStop = false;
            this.callsGroupBox.Text = "Calls";
            // 
            // userDataValueTextBox
            // 
            this.userDataValueTextBox.Location = new System.Drawing.Point(262, 56);
            this.userDataValueTextBox.Name = "userDataValueTextBox";
            this.userDataValueTextBox.Size = new System.Drawing.Size(92, 20);
            this.userDataValueTextBox.TabIndex = 15;
            this.userDataValueTextBox.Text = "value";
            // 
            // updateUserDataButton
            // 
            this.updateUserDataButton.Location = new System.Drawing.Point(360, 28);
            this.updateUserDataButton.Name = "updateUserDataButton";
            this.updateUserDataButton.Size = new System.Drawing.Size(115, 23);
            this.updateUserDataButton.TabIndex = 14;
            this.updateUserDataButton.Text = "Update User Data";
            this.updateUserDataButton.UseVisualStyleBackColor = true;
            this.updateUserDataButton.Click += new System.EventHandler(this.updateUserDataButton_Click);
            // 
            // userDataKeyTextBox
            // 
            this.userDataKeyTextBox.Location = new System.Drawing.Point(262, 30);
            this.userDataKeyTextBox.Name = "userDataKeyTextBox";
            this.userDataKeyTextBox.Size = new System.Drawing.Size(92, 20);
            this.userDataKeyTextBox.TabIndex = 12;
            this.userDataKeyTextBox.Text = "key";
            // 
            // userDataGrid
            // 
            this.userDataGrid.HelpVisible = false;
            this.userDataGrid.Location = new System.Drawing.Point(262, 152);
            this.userDataGrid.Name = "userDataGrid";
            this.userDataGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.userDataGrid.Size = new System.Drawing.Size(234, 152);
            this.userDataGrid.TabIndex = 11;
            this.userDataGrid.ToolbarVisible = false;
            // 
            // optionLabel
            // 
            this.optionLabel.AutoSize = true;
            this.optionLabel.Location = new System.Drawing.Point(18, 316);
            this.optionLabel.Name = "optionLabel";
            this.optionLabel.Size = new System.Drawing.Size(94, 13);
            this.optionLabel.TabIndex = 21;
            this.optionLabel.Text = "[user option value]";
            // 
            // genesysCallManager
            // 
            this.genesysCallManager.User = this.genesysUser;
            // 
            // genesysDevice
            // 
            this.genesysDevice.DeviceIndex = 0;
            this.genesysDevice.User = this.genesysUser;
            // 
            // genesysConnection
            // 
            this.genesysConnection.Password = "";
            this.genesysConnection.ServerUri = global::Genesys.WebServicesClient.Sample.Agent.WinForms.Properties.Settings.Default.GenesysServerUri;
            this.genesysConnection.Username = "";
            // 
            // genesysUser
            // 
            this.genesysUser.Connection = this.genesysConnection;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(717, 478);
            this.Controls.Add(this.optionLabel);
            this.Controls.Add(this.callsGroupBox);
            this.Controls.Add(this.connectionGroupBox);
            this.Controls.Add(this.deviceGroupBox);
            this.Controls.Add(this.userGroupBox);
            this.Controls.Add(this.errorsLabel);
            this.Name = "MainForm";
            this.Text = "Genesys Agent";
            ((System.ComponentModel.ISupportInitialize)(this.callsDataGrid)).EndInit();
            this.userGroupBox.ResumeLayout(false);
            this.deviceGroupBox.ResumeLayout(false);
            this.deviceGroupBox.PerformLayout();
            this.connectionGroupBox.ResumeLayout(false);
            this.connectionGroupBox.PerformLayout();
            this.callsGroupBox.ResumeLayout(false);
            this.callsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Components.GenesysConnection genesysConnection;
        private System.Windows.Forms.Button readyButton;
        private System.Windows.Forms.Button notReadyButton;
        private System.Windows.Forms.Label errorsLabel;
        private Components.GenesysCallManager genesysCallManager;
        private System.Windows.Forms.Label activeCallStateLabel;
        private System.Windows.Forms.DataGridView callsDataGrid;
        private System.Windows.Forms.Button answerButton;
        private System.Windows.Forms.Button hangupButton;
        private System.Windows.Forms.Button initiateTransferButton;
        private System.Windows.Forms.Button completeTransferButton;
        private System.Windows.Forms.TextBox phoneNumberTextBox;
        private System.Windows.Forms.Label activeCallIdLabel;
        private System.Windows.Forms.Label deviceStateLabel;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button disconnectButton;
        private Components.GenesysDevice genesysDevice;
        private System.Windows.Forms.Label deviceStateDisplayLabel;
        private System.Windows.Forms.GroupBox userGroupBox;
        private System.Windows.Forms.GroupBox deviceGroupBox;
        private System.Windows.Forms.GroupBox connectionGroupBox;
        private System.Windows.Forms.GroupBox callsGroupBox;
        private System.Windows.Forms.PropertyGrid userDataGrid;
        private System.Windows.Forms.TextBox userDataValueTextBox;
        private System.Windows.Forms.Button updateUserDataButton;
        private System.Windows.Forms.TextBox userDataKeyTextBox;
        private System.Windows.Forms.Label optionLabel;
        private Components.GenesysUser genesysUser;
    }
}

