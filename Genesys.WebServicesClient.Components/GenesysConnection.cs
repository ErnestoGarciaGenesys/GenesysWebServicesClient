using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public enum ConnectionState
    {
        Close, Opening, Open
    }

    public class GenesysConnection : NotifyPropertyChangedComponent
    {
        GenesysClient client;
        GenesysEventReceiver eventReceiver;
        ConnectionState connectionState;
        readonly AwaitingActivate awaitingOpen = new AwaitingActivate();

        internal event EventHandler<EventArgs> ConnectionStateChanged;

        [Category("Connection")]
        public string ServerUri { get; set; }

        // Username is spelled altogether (not userName), as in http://docs.genesys.com/Documentation/HTCC/8.5.2/API/GetUserInfo
        [Category("Connection")]
        public string Username { get; set; }

        [Category("Connection")]
        public string Password { get; set; }

        int openTimeoutMs = 10000;
        [Category("Connection"), DefaultValue(10000)]
        public int OpenTimeoutMs
        {
            get { return openTimeoutMs; }
            set { openTimeoutMs = value; }
        }

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

        public async Task OpenAsync()
        {
            if (connectionState == ConnectionState.Close)
            {
                try
                {
                    SetConnectionState(ConnectionState.Opening);

                    client = new GenesysClient.Setup()
                    {
                        ServerUri = ServerUri,
                        UserName = Username,
                        Password = Password,
                    }
                    .Create();

                    eventReceiver = client.CreateEventReceiver();

                    await Task.Factory.StartNew(
                        () => eventReceiver.Open(OpenTimeoutMs),
                        TaskCreationOptions.LongRunning);

                    SetConnectionState(ConnectionState.Open);

                    awaitingOpen.Complete(null);
                }
                catch (Exception e)
                {
                    Close();
                    awaitingOpen.Complete(e);
                }
            }
            else if (connectionState == ConnectionState.Opening)
            {
                await awaitingOpen.Await();
            }
            else
            {
                // Connection is open. Do nothing
            }
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

            SetConnectionState(ConnectionState.Close);
        }

        void SetConnectionState(ConnectionState s)
        {
            if (connectionState != s)
            {
                connectionState = s;

                if (ConnectionStateChanged != null)
                    ConnectionStateChanged(this, EventArgs.Empty);

                RaisePropertyChanged("ConnectionState");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Close();

            base.Dispose(disposing);
        }

        [Browsable(false)]
        public GenesysClient Client
        {
            get { return client; }
        }

        [Browsable(false)]
        public GenesysEventReceiver EventReceiver
        {
            get { return eventReceiver; }
        }

        [ReadOnly(true), Browsable(false)]
        public ConnectionState ConnectionState
        {
            get { return connectionState; }
        }
    }
}
