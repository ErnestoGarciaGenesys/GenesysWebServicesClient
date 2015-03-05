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
        // Needs disposal
        IEventSubscription eventSubscription;

        /// <summary>
        /// Available means that this object has been correctly initialized and all its
        /// resource properties and methods are available to use.
        /// </summary>
        public bool Available { get { return InternalActivationStage == ActivationStage.Started; } }

        public event EventHandler AvailableChanged;

        #region Initialization Properties

        [Category("Initialization")]
        public GenesysConnection Connection
        {
            get { return (GenesysConnection)Parent; }
            set
            {
                if (InternalActivationStage != ActivationStage.Idle)
                    throw new InvalidOperationException("Property must be set while component is not started");
                
                Parent = value;
            }
        }

        #endregion Initialization Properties

        protected override Exception CanStart()
        {
            if (Connection == null)
                return new InvalidOperationException("Connection property must be set");

            if (Connection.ConnectionState != ConnectionState.Open)
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
            StartHierarchyUpdate();
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

        protected override void OnParentUpdated(InternalUpdatedEventArgs e)
        {
            if (Connection.ConnectionState == ConnectionState.Open && AutoRecover)
                Start();

            if (Connection.ConnectionState == ConnectionState.Close)
                Stop();
        }

        void HandleEvent(object sender, GenesysEvent genesysEvent)
        {
            UserResource = genesysEvent.GetResourceAsTypeOrNull<UserResource>("user");
            StartHierarchyUpdate(genesysEvent);
            RaiseUpdated();
        }

        public event EventHandler Updated;

        void RaiseUpdated()
        {
            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }

        #region Internal

        public UserResource UserResource { get; private set; }

        #endregion Internal

        public IDictionary<string, IDictionary<string, object>> Settings { get; private set; }

        #region Operations

        public Task ChangeState(string value)
        {
            return Connection.InternalClient.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        #endregion Operations
    }
}
