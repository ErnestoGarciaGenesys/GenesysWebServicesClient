using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;

namespace Genesys.WebServicesClient.Components.Test
{
    [TestClass]
    public class GenesysConnectionTest
    {
        [TestMethod]
        public void initialize_GenesysUser_then_GenesysConnection()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            
            Console.WriteLine(SynchronizationContext.Current);

            var connection = new GenesysConnection();
            var user = new GenesysUser();
            ((ISupportInitialize)connection).BeginInit();
            ((ISupportInitialize)user).BeginInit();

            connection.ServerUri = "http://testUri";
            connection.Password = "testPassword";
            connection.Username = "testUsername";

            user.Connection = connection;

            ((ISupportInitialize)connection).EndInit();
            ((ISupportInitialize)user).EndInit();
        }
    }
}
