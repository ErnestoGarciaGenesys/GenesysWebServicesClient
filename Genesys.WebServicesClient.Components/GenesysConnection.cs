using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysConnection : ActiveComponent
    {
        GenesysClient client;
        GenesysEventReceiver eventReceiver;

        [Category("Connection")]
        public string ServerUri { get; set; }

        // Username is spelled altogether (not userName), as in http://docs.genesys.com/Documentation/HTCC/8.5.2/API/GetUserInfo
        [Category("Connection")]
        public string Username { get; set; }
        
        [Category("Connection")]
        public string Password { get; set; }

        // Reimplementing property for giving different attributes and implementation.
        [ReadOnly(true), Browsable(false), DefaultValue(false)]
        public override bool AutoActivate
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

        protected override void ActivateImpl()
        {
            client = new GenesysClient.Setup()
            {
                ServerUri = ServerUri,
                UserName = Username,
                Password = Password
            }
            .Create();

            eventReceiver = client.CreateEventReceiver();
            eventReceiver.Open();
        }

        protected override void DeactivateImpl()
        {
            eventReceiver.Dispose();
            eventReceiver = null;

            client.Dispose();
            client = null;
        }

        internal GenesysClient Client
        {
            get { return client; }
        }

        internal GenesysEventReceiver EventReceiver
        {
            get { return eventReceiver; }
        }
    }
}
