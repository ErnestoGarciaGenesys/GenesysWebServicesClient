using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysConnection : AutoInitComponent
    {
        GenesysClient client;

        [Category("Connection")]
        public string ServerUri { get; set; }

        // Username is spelled altogether (not userName), as in http://docs.genesys.com/Documentation/HTCC/8.5.2/API/GetUserInfo
        [Category("Connection")]
        public string Username { get; set; }
        
        [Category("Connection")]
        public string Password { get; set; }

        // Reimplementing property for giving different attributes and implementation.
        [ReadOnly(true), Browsable(false), DefaultValue(false)]
        public override bool AutoInitialize
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// When using this constructor, this instance must be disposed explicitly.
        /// </summary>
        public GenesysConnection()
        {
            isParent = true;
        }

        /// <summary>
        /// When using this constructor, this instance will be automatically disposed by the parent container.
        /// </summary>
        public GenesysConnection(IContainer container)
            : this()
        {
            container.Add(this);
        }

        protected override void InitializeImpl()
        {
            client = new GenesysClient.Setup()
            {
                ServerUri = ServerUri,
                UserName = Username,
                Password = Password
            }
            .Create();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }

        internal GenesysClient Client
        {
            get { return client; }
        }
    }
}
