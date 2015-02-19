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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Genesys.WebServicesClient.Sample.Agent.WPF
{
    public partial class ToastWindow : Window
    {
        readonly GenesysCall call;

        public ToastWindow()
        {
            InitializeComponent();
        }

        public ToastWindow(GenesysCall call)
            : this()
        {
            this.call = call;
            this.DataContext = call;

            var workArea = Screen.PrimaryScreen.WorkingArea;
            this.Left = workArea.Right - this.Width - 16;
            this.Top = workArea.Bottom - this.Height - 32;
        }

        void Answer_Click(object sender, RoutedEventArgs e)
        {
            call.Answer();
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
