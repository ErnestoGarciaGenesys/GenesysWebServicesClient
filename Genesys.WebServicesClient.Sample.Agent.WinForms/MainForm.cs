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

            Application.ThreadException += (s, e) =>
                errorsLabel.Text = e.Exception.ToString();

            // Varios problemas:
            // - Para hacer DataBinding parece que todas las propiedades en el Path deben estar tipadas y estar definidas, al menos
            //  por medio de un ICustomTypeDescriptor.
            // - Si una de las propiedades es null, el binding da error al no poder registrar un propDesc.AddValueChanged().
            //  A no ser que se use un BindingSource
            // - Windows Forms no admite binding con objetos dinámicos (IDynamicMetaObjectProvider, ExpandoObject...)

            CreateBinding(deviceStateLabel, "Text", genesysDevice, "UserState.State");
            CreateBinding(deviceStateDisplayLabel, "Text", genesysDevice, "UserState.DisplayName");

            callsDataGrid.DataSource = genesysCallManager.Calls;

            CreateBinding(activeCallIdLabel, "Text", genesysCallManager, "ActiveCall.Id");
            CreateBinding(activeCallStateLabel, "Text", genesysCallManager, "ActiveCall.State");

            CreateBinding(answerButton, "Enabled", genesysCallManager, "ActiveCall.AnswerCapable");
            CreateBinding(hangupButton, "Enabled", genesysCallManager, "ActiveCall.HangupCapable");

            CreateBinding(initiateTransferButton, "Enabled", genesysCallManager, "InitiateTransferCapable");
            CreateBinding(completeTransferButton, "Enabled", genesysCallManager, "CompleteTransferCapable");

            genesysUser.ResourceUpdated += (s, e) => UpdateUserDataGrid();

            genesysConnection.ActiveChanged += (s, e) => RefreshConnectionComponents();
            RefreshConnectionComponents();
        }

        Binding CreateBinding(Control control, string propertyName, object dataSource, string dataMember)
        {
            bool isPath = dataMember.Contains('.');
            if (isPath)
            {
                // A plain Binding will not work with null values in the path, so wrap the dataSource in a BindingSource,
                // which will take care of null values appropriately.
                // Notice that the BindingSource only wraps the dataSource, not the dataMember.
                var bindingSource = new BindingSource();
                bindingSource.DataSource = dataSource;
                dataSource = bindingSource;
            }

            var binding = control.DataBindings.Add(propertyName, dataSource, dataMember,
                formattingEnabled: true, 
                updateMode: DataSourceUpdateMode.OnPropertyChanged);

            binding.BindingComplete += (s, e) =>
                {
                    if (e.BindingCompleteState != BindingCompleteState.Success)
                        Console.WriteLine(e.ErrorText);
                };
            
            if (isPath)
                // The BindingSource does not do an initial update if one of the DataMember properties in its path is null,
                // that is why an initial update is requested explicitely here.
                binding.ReadValue();
            
            return binding;
        }

        void RefreshConnectionComponents()
        {
            usernameTextBox.Enabled = !genesysConnection.Active;
            passwordTextBox.Enabled = !genesysConnection.Active;
            connectButton.Enabled = !genesysConnection.Active;
            disconnectButton.Enabled = genesysConnection.Active;
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

        void connectButton_Click(object sender, EventArgs e)
        {
            genesysConnection.Username = usernameTextBox.Text;
            genesysConnection.Password = passwordTextBox.Text;
            genesysConnection.Activate();
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
            genesysCallManager.InitiateTransfer(phoneNumberTextBox.Text);
        }

        void completeTransferButton_Click(object sender, EventArgs e)
        {
            genesysCallManager.CompleteTransfer();
        }
    }
}
