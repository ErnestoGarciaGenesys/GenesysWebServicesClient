using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Genesys.WebServicesClient.Components.Test
{
    [TestClass]
    public class UsabilityTests
    {
        [TestMethod]
        public async void initialize_connection_and_users_sync()
        {
            var connection = new GenesysConnection()
            {
                ServerUri = "http://localhost:5088",
                Username = "paveld@redwings.com",
                Password = "password",
            };

            var user1 = new GenesysUser()
            {
                Connection = connection
            };

            var user2 = new GenesysUser()
            {
                Connection = connection
            };

            await connection.OpenAsync();
            try
            {
                //user1.Activate();
            }
            catch (Exception)
            {
                //user1.Deactivate();
                //connection.Close();
                throw;
            }
        }

        [TestMethod]
        public void initialize_connection_and_users_async()
        {
            var connection = new GenesysConnection()
            {
                ServerUri = "http://localhost:5088",
                Username = "paveld@redwings.com",
                Password = "password",
            };

            var user1 = new GenesysUser()
            {
                Connection = connection
            };

            var user2 = new GenesysUser()
            {
                Connection = connection
            };

            //await connection.OpenAsync();
            //await user1.ActivateAsync();
        }
    }
}
