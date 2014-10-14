using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

            Binding stateBinding = stateLabel.DataBindings.Add("Text", genesysAgent, "State");
        }
        
        void MainForm_Load(object sender, EventArgs e)
        {
            genesysAgent.Initialize();
            //    genesysConnection.Initialize();
        }

        async void readyButton_Click(object sender, EventArgs e)
        {
            try
            {
                await genesysAgent.MakeReady();
                errorsLabel.Text = "";
            }
            catch (Exception ex)
            {
                errorsLabel.Text = ex.ToString();
            }
        }

        async void notReadyButton_Click(object sender, EventArgs e)
        {
            try
            {
                await genesysAgent.MakeNotReady();
                errorsLabel.Text = "";
            }
            catch (Exception ex)
            {
                errorsLabel.Text = ex.ToString();
            }
        }
    }
}
