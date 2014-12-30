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
            this.stateLabel = new System.Windows.Forms.Label();
            this.readyButton = new System.Windows.Forms.Button();
            this.notReadyButton = new System.Windows.Forms.Button();
            this.errorsLabel = new System.Windows.Forms.Label();
            this.callStateLabel = new System.Windows.Forms.Label();
            this.callsDataGrid = new System.Windows.Forms.DataGridView();
            this.answerButton = new System.Windows.Forms.Button();
            this.hangupButton = new System.Windows.Forms.Button();
            this.initiateTransferButton = new System.Windows.Forms.Button();
            this.completeTransferButton = new System.Windows.Forms.Button();
            this.phoneNumberTextBox = new System.Windows.Forms.TextBox();
            this.activeCallIdLabel = new System.Windows.Forms.Label();
            this.agentStateLabel = new System.Windows.Forms.Label();
            this.genesysUser = new Genesys.WebServicesClient.Components.GenesysUser();
            this.genesysConnection = new Genesys.WebServicesClient.Components.GenesysConnection(this.components);
            this.genesysCallManager = new Genesys.WebServicesClient.Components.GenesysCallManager();
            this.connectButton = new System.Windows.Forms.Button();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.disconnectButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.callsDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.genesysUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.genesysConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.genesysCallManager)).BeginInit();
            this.SuspendLayout();
            // 
            // stateLabel
            // 
            this.stateLabel.AutoSize = true;
            this.stateLabel.Location = new System.Drawing.Point(16, 179);
            this.stateLabel.Name = "stateLabel";
            this.stateLabel.Size = new System.Drawing.Size(38, 13);
            this.stateLabel.TabIndex = 0;
            this.stateLabel.Text = "[State]";
            // 
            // readyButton
            // 
            this.readyButton.Location = new System.Drawing.Point(16, 240);
            this.readyButton.Name = "readyButton";
            this.readyButton.Size = new System.Drawing.Size(75, 23);
            this.readyButton.TabIndex = 1;
            this.readyButton.Text = "Ready";
            this.readyButton.UseVisualStyleBackColor = true;
            this.readyButton.Click += new System.EventHandler(this.readyButton_Click);
            // 
            // notReadyButton
            // 
            this.notReadyButton.Location = new System.Drawing.Point(16, 270);
            this.notReadyButton.Name = "notReadyButton";
            this.notReadyButton.Size = new System.Drawing.Size(75, 23);
            this.notReadyButton.TabIndex = 2;
            this.notReadyButton.Text = "Not Ready";
            this.notReadyButton.UseVisualStyleBackColor = true;
            this.notReadyButton.Click += new System.EventHandler(this.notReadyButton_Click);
            // 
            // errorsLabel
            // 
            this.errorsLabel.AutoSize = true;
            this.errorsLabel.ForeColor = System.Drawing.Color.DarkRed;
            this.errorsLabel.Location = new System.Drawing.Point(16, 332);
            this.errorsLabel.Name = "errorsLabel";
            this.errorsLabel.Size = new System.Drawing.Size(40, 13);
            this.errorsLabel.TabIndex = 3;
            this.errorsLabel.Text = "[Errors]";
            // 
            // callStateLabel
            // 
            this.callStateLabel.AutoSize = true;
            this.callStateLabel.Location = new System.Drawing.Point(156, 13);
            this.callStateLabel.Name = "callStateLabel";
            this.callStateLabel.Size = new System.Drawing.Size(38, 13);
            this.callStateLabel.TabIndex = 0;
            this.callStateLabel.Text = "[State]";
            // 
            // callsDataGrid
            // 
            this.callsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.callsDataGrid.Location = new System.Drawing.Point(158, 168);
            this.callsDataGrid.Name = "callsDataGrid";
            this.callsDataGrid.Size = new System.Drawing.Size(240, 150);
            this.callsDataGrid.TabIndex = 4;
            // 
            // answerButton
            // 
            this.answerButton.Location = new System.Drawing.Point(159, 72);
            this.answerButton.Name = "answerButton";
            this.answerButton.Size = new System.Drawing.Size(75, 23);
            this.answerButton.TabIndex = 5;
            this.answerButton.Text = "Answer";
            this.answerButton.UseVisualStyleBackColor = true;
            this.answerButton.Click += new System.EventHandler(this.answerButton_Click);
            // 
            // hangupButton
            // 
            this.hangupButton.Location = new System.Drawing.Point(241, 72);
            this.hangupButton.Name = "hangupButton";
            this.hangupButton.Size = new System.Drawing.Size(75, 23);
            this.hangupButton.TabIndex = 6;
            this.hangupButton.Text = "Hang up";
            this.hangupButton.UseVisualStyleBackColor = true;
            this.hangupButton.Click += new System.EventHandler(this.hangupButton_Click);
            // 
            // initiateTransferButton
            // 
            this.initiateTransferButton.Location = new System.Drawing.Point(159, 102);
            this.initiateTransferButton.Name = "initiateTransferButton";
            this.initiateTransferButton.Size = new System.Drawing.Size(110, 23);
            this.initiateTransferButton.TabIndex = 7;
            this.initiateTransferButton.Text = "Initiate transfer";
            this.initiateTransferButton.UseVisualStyleBackColor = true;
            this.initiateTransferButton.Click += new System.EventHandler(this.initiateTransferButton_Click);
            // 
            // completeTransferButton
            // 
            this.completeTransferButton.Location = new System.Drawing.Point(159, 132);
            this.completeTransferButton.Name = "completeTransferButton";
            this.completeTransferButton.Size = new System.Drawing.Size(110, 23);
            this.completeTransferButton.TabIndex = 8;
            this.completeTransferButton.Text = "Complete Transfer";
            this.completeTransferButton.UseVisualStyleBackColor = true;
            this.completeTransferButton.Click += new System.EventHandler(this.completeTransferButton_Click);
            // 
            // phoneNumberTextBox
            // 
            this.phoneNumberTextBox.Location = new System.Drawing.Point(275, 104);
            this.phoneNumberTextBox.Name = "phoneNumberTextBox";
            this.phoneNumberTextBox.Size = new System.Drawing.Size(75, 20);
            this.phoneNumberTextBox.TabIndex = 9;
            // 
            // activeCallIdLabel
            // 
            this.activeCallIdLabel.AutoSize = true;
            this.activeCallIdLabel.Location = new System.Drawing.Point(156, 47);
            this.activeCallIdLabel.Name = "activeCallIdLabel";
            this.activeCallIdLabel.Size = new System.Drawing.Size(72, 13);
            this.activeCallIdLabel.TabIndex = 10;
            this.activeCallIdLabel.Text = "[ActiveCall Id]";
            // 
            // agentStateLabel
            // 
            this.agentStateLabel.AutoSize = true;
            this.agentStateLabel.Location = new System.Drawing.Point(16, 201);
            this.agentStateLabel.Name = "agentStateLabel";
            this.agentStateLabel.Size = new System.Drawing.Size(60, 13);
            this.agentStateLabel.TabIndex = 13;
            this.agentStateLabel.Text = "[StateNew]";
            // 
            // genesysUser
            // 
            this.genesysUser.Connection = this.genesysConnection;
            // 
            // genesysConnection
            // 
            this.genesysConnection.Password = "";
            this.genesysConnection.ServerUri = global::Genesys.WebServicesClient.Sample.Agent.WinForms.Properties.Settings.Default.GenesysServerUri;
            this.genesysConnection.Username = "";
            // 
            // genesysCallManager
            // 
            this.genesysCallManager.User = this.genesysUser;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(12, 62);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 14;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(12, 10);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(100, 20);
            this.usernameTextBox.TabIndex = 15;
            this.usernameTextBox.Text = "paveld@redwings.com";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(12, 36);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(100, 20);
            this.passwordTextBox.TabIndex = 16;
            this.passwordTextBox.Text = "password";
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(12, 91);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectButton.TabIndex = 14;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(440, 478);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.agentStateLabel);
            this.Controls.Add(this.activeCallIdLabel);
            this.Controls.Add(this.phoneNumberTextBox);
            this.Controls.Add(this.completeTransferButton);
            this.Controls.Add(this.initiateTransferButton);
            this.Controls.Add(this.hangupButton);
            this.Controls.Add(this.answerButton);
            this.Controls.Add(this.callsDataGrid);
            this.Controls.Add(this.errorsLabel);
            this.Controls.Add(this.notReadyButton);
            this.Controls.Add(this.readyButton);
            this.Controls.Add(this.callStateLabel);
            this.Controls.Add(this.stateLabel);
            this.Name = "MainForm";
            this.Text = "Genesys Agent";
            ((System.ComponentModel.ISupportInitialize)(this.callsDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.genesysUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.genesysConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.genesysCallManager)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label stateLabel;
        private Components.GenesysConnection genesysConnection;
        private Components.GenesysUser genesysUser;
        private System.Windows.Forms.Button readyButton;
        private System.Windows.Forms.Button notReadyButton;
        private System.Windows.Forms.Label errorsLabel;
        private Components.GenesysCallManager genesysCallManager;
        private System.Windows.Forms.Label callStateLabel;
        private System.Windows.Forms.DataGridView callsDataGrid;
        private System.Windows.Forms.Button answerButton;
        private System.Windows.Forms.Button hangupButton;
        private System.Windows.Forms.Button initiateTransferButton;
        private System.Windows.Forms.Button completeTransferButton;
        private System.Windows.Forms.TextBox phoneNumberTextBox;
        private System.Windows.Forms.Label activeCallIdLabel;
        private System.Windows.Forms.Label agentStateLabel;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button disconnectButton;
    }
}

