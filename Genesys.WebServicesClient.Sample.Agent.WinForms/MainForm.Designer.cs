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
            this.stateErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.genesysConnection = new Genesys.WebServicesClient.Components.GenesysConnection();
            this.genesysAgent = new Genesys.WebServicesClient.Components.GenesysAgent();
            ((System.ComponentModel.ISupportInitialize)(this.stateErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // stateLabel
            // 
            this.stateLabel.AutoSize = true;
            this.stateLabel.Location = new System.Drawing.Point(13, 13);
            this.stateLabel.Name = "stateLabel";
            this.stateLabel.Size = new System.Drawing.Size(38, 13);
            this.stateLabel.TabIndex = 0;
            this.stateLabel.Text = "[State]";
            // 
            // readyButton
            // 
            this.readyButton.Location = new System.Drawing.Point(16, 42);
            this.readyButton.Name = "readyButton";
            this.readyButton.Size = new System.Drawing.Size(75, 23);
            this.readyButton.TabIndex = 1;
            this.readyButton.Text = "Ready";
            this.readyButton.UseVisualStyleBackColor = true;
            this.readyButton.Click += new System.EventHandler(this.readyButton_Click);
            // 
            // notReadyButton
            // 
            this.notReadyButton.Location = new System.Drawing.Point(16, 72);
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
            this.errorsLabel.Location = new System.Drawing.Point(12, 110);
            this.errorsLabel.Name = "errorsLabel";
            this.errorsLabel.Size = new System.Drawing.Size(0, 13);
            this.errorsLabel.TabIndex = 3;
            // 
            // stateErrorProvider
            // 
            this.stateErrorProvider.ContainerControl = this;
            // 
            // genesysConnection
            // 
            this.genesysConnection.Password = "password";
            this.genesysConnection.ServerUri = "http://localhost:5088";
            this.genesysConnection.Username = "paveld@redwings.com";
            // 
            // genesysAgent
            // 
            this.genesysAgent.Connection = this.genesysConnection;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.errorsLabel);
            this.Controls.Add(this.notReadyButton);
            this.Controls.Add(this.readyButton);
            this.Controls.Add(this.stateLabel);
            this.Name = "MainForm";
            this.Text = "Genesys Agent";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.stateErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label stateLabel;
        private Components.GenesysConnection genesysConnection;
        private Components.GenesysAgent genesysAgent;
        private System.Windows.Forms.Button readyButton;
        private System.Windows.Forms.Button notReadyButton;
        private System.Windows.Forms.Label errorsLabel;
        private System.Windows.Forms.ErrorProvider stateErrorProvider;
    }
}

