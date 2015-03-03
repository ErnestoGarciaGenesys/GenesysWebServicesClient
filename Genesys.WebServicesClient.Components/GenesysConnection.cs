using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysConnection : StartComponent
    {
        GenesysClient client;
        GenesysEventReceiver eventReceiver;

        #region Initialization Properties

        [Category("Initialization")]
        public string ServerUri { get; set; }

        // Username is spelled altogether (not userName), as in http://docs.genesys.com/Documentation/HTCC/8.5.2/API/GetUserInfo
        [Category("Initialization")]
        public string Username { get; set; }

        [Category("Initialization")]
        public string Password { get; set; }

        int openTimeoutMs = 10000;
        [Category("Initialization"), DefaultValue(10000)]
        public int OpenTimeoutMs
        {
            get { return openTimeoutMs; }
            set { openTimeoutMs = value; }
        }

        #endregion Initialization Properties

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

        protected override async Task StartImplAsync(CancellationToken cancellationToken)
        {
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
        }

        protected override void StopImpl()
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
        }

        #region Observable Properties

        [Browsable(false)]
        public ConnectionState ConnectionState
        {
            get { return ToState(activationStage); }
        }

        ConnectionState ToState(ActivationStage s)
        {
            switch (s)
            {
                case ActivationStage.Idle:
                    return ConnectionState.Close;
                case ActivationStage.Started:
                    return ConnectionState.Open;
                case ActivationStage.Starting:
                default:
                    return ConnectionState.Opening;
            }
        }

        protected override void OnActivationStageChanged()
        {
            base.OnActivationStageChanged();
            RaisePropertyChanged("ConnectionState");
        }

        #endregion Observable Properties

        #region Internal

        [Browsable(false)]
        public GenesysClient InternalClient
        {
            get
            {
                if (client == null)
                    throw new InvalidOperationException("Connection is closed");

                return client;
            }
        }

        [Browsable(false)]
        public GenesysEventReceiver InternalEventReceiver
        {
            get
            {
                if (client == null)
                    throw new InvalidOperationException("Connection is closed");

                return eventReceiver;
            }
        }

        #endregion Internal
    }

    public enum ConnectionState
    {
        Close,
        Opening,
        Open
    }
}
