using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysConnection : ActiveGenesysComponent
    {
        GenesysClient client;
        GenesysEventReceiver eventReceiver;

        #region Initialization Properties

        [Category("Initialization")]
        public string ServerUri { get; set; }

        [Category("Initialization")]
        public string UserName { get; set; }

        [Category("Initialization")]
        public string Password { get; set; }

        int openTimeoutMs = 10000;
        [Category("Initialization"), DefaultValue(10000)]
        public int OpenTimeoutMs
        {
            get { return openTimeoutMs; }
            set { openTimeoutMs = value; }
        }

        bool webSocketsEnabled = true;
        [Category("Initialization"), DefaultValue(true)]
        public bool WebSocketsEnabled
        {
            get { return webSocketsEnabled; }
            set { webSocketsEnabled = value; }
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
                UserName = UserName,
                Password = Password,
            }
            .Create();

            eventReceiver = client.CreateEventReceiver(new GenesysEventReceiver.Setup()
                {
                    WebSocketsEnabled = webSocketsEnabled,
                });

            await eventReceiver.OpenAsync(OpenTimeoutMs, cancellationToken);
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

        [ReadOnly(true)]
        public ConnectionState ConnectionState
        {
            get { return ToConnectionState(InternalActivationStage); }
        }

        ConnectionState ToConnectionState(ActivationStage s)
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

            StartHierarchyUpdate(doLast: postEvents =>
                RaisePropertyChanged(postEvents, "ConnectionState"));
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
                if (eventReceiver == null)
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
