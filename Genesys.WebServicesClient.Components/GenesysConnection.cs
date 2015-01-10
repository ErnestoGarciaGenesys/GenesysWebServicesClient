using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public enum ConnectionState
    {
        Open, Close
    }

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

        [Category("Connection"), DefaultValue(5000)]
        public int OpenTimeoutMs { get; set; }

        /// <summary>
        /// When using this constructor, this instance must be disposed explicitly.
        /// </summary>
        public GenesysConnection() { }

        /// <summary>
        /// When using this constructor, this instance will be automatically disposed by the parent container.
        /// </summary>
        public GenesysConnection(IContainer container)
            : this()
        {
            container.Add(this);
        }

        [ReadOnly(true), Browsable(false)]
        public ConnectionState ConnectionState { get; private set; }

        internal event EventHandler<EventArgs> ConnectionStateChanged;

        public void Open()
        {
            Close();

            client = new GenesysClient.Setup()
            {
                ServerUri = ServerUri,
                UserName = Username,
                Password = Password
            }
            .Create();

            eventReceiver = client.CreateEventReceiver();

            // TODO: do asynchronously in a background thread: Task.Factory.StartNew(..., TaskCreationOptions.LongRunning)?
            // And convert this method in ConnectAsync, returning a Task.
            eventReceiver.Open(OpenTimeoutMs);

            ConnectionState = ConnectionState.Open;
            if (ConnectionStateChanged != null)
                ConnectionStateChanged(this, EventArgs.Empty);        
        }

        public void Close()
        {
            if (eventReceiver != null)
            {
                eventReceiver.Dispose();
                eventReceiver = null;
            }

            if (client != null)
            {
                client.Dispose();
                client = null;
            }

            if (ConnectionState == ConnectionState.Open)
            {
                ConnectionState = ConnectionState.Close;
                if (ConnectionStateChanged != null)
                    ConnectionStateChanged(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Close();

            base.Dispose(disposing);
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
