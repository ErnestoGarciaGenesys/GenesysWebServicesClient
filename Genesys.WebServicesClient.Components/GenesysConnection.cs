using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysConnection : IComponent
    {
        public string ServerUri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        internal GenesysClient Client { get; private set; }

        public GenesysConnection(String serverUri, String username, String password)
        {
            this.ServerUri = serverUri;
            this.Username = username;
            this.Password = password;
        }

        public GenesysConnection() { }

        public void Initialize() {
            Client = new GenesysClient.Setup()
            {
                ServerUri = ServerUri,
                UserName = Username,
                Password = Password
            }
            .Create();
        }
        
        #region IComponent

        public event EventHandler Disposed;

        public ISite Site { get; set; }

        public void Dispose() { }

        #endregion IComponent
    }
}
