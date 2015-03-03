using Genesys.WebServicesClient.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysUser : ActiveGenesysComponent
    {
        GenesysConnection connection;

        // Needs disposal
        IEventSubscription eventSubscription;

        /// <summary>
        /// Available means that this object has been correctly initialized and all its
        /// resource properties and methods are available to use.
        /// </summary>
        public bool Available { get { return activationStage == ActivationStage.Started; } }

        public event EventHandler AvailableChanged;

        #region Initialization Properties

        [Category("Initialization")]
        public GenesysConnection Connection
        {
            get { return connection; }
            set
            {
                if (value != connection)
                {
                    if (activationStage != ActivationStage.Idle)
                        throw new InvalidOperationException("Property must be set while component is not started");

                    if (value == null)
                    {
                        connection.InternalActivationStageChanged -= genesysConnection_InternalActivationStageChanged;
                        connection = null;
                    }
                    else
                    {
                        connection = value;
                        connection.InternalActivationStageChanged += genesysConnection_InternalActivationStageChanged;
                    }
                }
            }
        }

        #endregion Initialization Properties

        protected override Exception CanStart()
        {
            if (connection == null)
                return new InvalidOperationException("Connection property must be set");

            if (connection.InternalActivationStage != ActivationStage.Started)
                return new ActivationException("Connection is not open");

            return null;
        }

        protected override async Task StartImplAsync(CancellationToken cancellationToken)
        {
            eventSubscription = Connection.InternalEventReceiver.SubscribeAll(HandleEvent);

            // Documentation about recovering existing state:
            // http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together

            var response =
                await Connection.InternalClient.CreateRequest("GET", "/api/v2/me?subresources=*")
                    .SendAsync<UserResourceResponse>(cancellationToken);
            
            LoadResource(response);
            RaiseResourceUpdated();
        }

        void LoadResource(IGenesysResponse<UserResourceResponse> response)
        {
            UserResource = response.AsType.user;

            var userResource = (IDictionary<string, object>)response.AsDictionary["user"];
            var untypedSettings = (IDictionary<string, object>)userResource["settings"];

            // Concretizing dictionary type to a dictionary of dictionaries,
            // because Settings contains sections, which contain key-value pairs.
            Settings = untypedSettings.ToDictionary(kvp => kvp.Key, kvp => (IDictionary<string, object>)kvp.Value);
        }

        protected override void StopImpl()
        {
            if (eventSubscription != null)
            {
                try
                {
                    eventSubscription.Dispose();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Event unsubscription failed: " + e);
                }

                eventSubscription = null;
            }
        }

        void genesysConnection_InternalActivationStageChanged(object sender, EventArgs e)
        {
            if (connection.InternalActivationStage == ActivationStage.Started && autoRecover)
                Start();

            if (connection.InternalActivationStage == ActivationStage.Idle)
                Stop();
        }

        void HandleEvent(object sender, GenesysEvent e)
        {
            UserResource = e.GetResourceAsTypeOrNull<UserResource>("user");

            if (InternalUpdated != null)
                InternalUpdated(this, new InternalUpdatedEventArgs(e));

            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }

        public event EventHandler Updated;

        #region Internal

        public UserResource UserResource { get; private set; }

        public event EventHandler<InternalUpdatedEventArgs> InternalUpdated;

        // TODO: include event or resource info
        void RaiseResourceUpdated()
        {
            if (InternalUpdated != null)
                InternalUpdated(this, new InternalUpdatedEventArgs());
        }

        #endregion Internal

        public IDictionary<string, IDictionary<string, object>> Settings { get; private set; }

        #region Operations

        public Task ChangeState(string value)
        {
            return Connection.InternalClient.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        #endregion Operations
    }

    public class InternalUpdatedEventArgs : EventArgs
    {
        public GenesysEvent GenesysEvent { get; private set; }

        public InternalUpdatedEventArgs()
        {
        }

        public InternalUpdatedEventArgs(GenesysEvent e)
            : this()
        {
            GenesysEvent = e;
        }

    }
}
