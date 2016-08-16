using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Genesys.WebServicesClient.Test
{
    [TestClass]
    public class GenesysClientTest
    {
        [TestMethod]
        public void Create_with_URI_succeeds()
        {
            new GenesysClient.Setup { ServerUri = "http://myserver", Anonymous = true }.Create();
        }

        [TestMethod]
        public void Create_with_HttpClient_succeeds()
        {
            new GenesysClient.Setup { HttpClient = new System.Net.Http.HttpClient(), Anonymous = true }.Create();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_with_no_setup_parameters_fails()
        {
            new GenesysClient.Setup().Create();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_with_no_credentials_fails()
        {
            new GenesysClient.Setup { ServerUri = "http://myserver" }.Create();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_with_no_password_fails()
        {
            new GenesysClient.Setup { ServerUri = "http://myserver", UserName = "myuser" }.Create();
        }
    }
}
