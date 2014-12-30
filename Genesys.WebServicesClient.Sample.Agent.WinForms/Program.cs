using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Sample.Agent.WinForms
{
    static class Program
    {
        static MainForm mainForm;

        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Program.mainForm = new MainForm();
            Application.Run(mainForm);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Console.WriteLine("Exception raised: {0}\n{1}", e.Exception, e.Exception.StackTrace);
        }
    }
}
