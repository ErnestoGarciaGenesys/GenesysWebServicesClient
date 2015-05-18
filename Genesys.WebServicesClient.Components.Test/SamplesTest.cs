using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Genesys.WebServicesClient.Components.Test
{
    [TestClass]
    public class SamplesTest
    {
        [TestMethod]
        public async Task get_contacts()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var connection = new GenesysConnection()
            {
                ServerUri = Configuration.ServerUri,
                UserName = Configuration.UserName,
                Password = Configuration.Password,
                WebSocketsEnabled = false,
            };

            await connection.StartAsync();

            var response = await connection.InternalClient.CreateRequest("GET", "/api/v2/contacts").SendAsync();

            Debug.WriteLine(response.AsString);
        }
    }
}
