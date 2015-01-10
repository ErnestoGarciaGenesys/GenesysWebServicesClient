using Genesys.WebServicesClient.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class InternalExceptionThrownEventArgs : EventArgs
    {
        readonly Exception exception;

        public InternalExceptionThrownEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        public Exception Exception
        {
            get { return exception; }
        }
    }

    public class GenesysUser : NotifyPropertyChangedComponent
    {
        public bool Active { get; private set; }

        GenesysConnection connection;

        [Category("Connection")]
        public GenesysConnection Connection
        {
            get { return connection; }
            set
            {
                if (value != connection)
                {
                    if (Active)
                        throw new InvalidOperationException("Cannot change connection while Active");

                    if (value == null)
                    {
                        connection.ConnectionStateChanged -= connection_ConnectionStateChanged;
                        connection = null;
                    }
                    else
                    {
                        connection = value;
                        connection.ConnectionStateChanged += connection_ConnectionStateChanged;
                    }
                }
            }
        }

        enum Stage { Idle, GettingResource, Available }

        Stage stage = Stage.Idle;

        // Valid during the GettingResource stage
        CancellationTokenSource gettingResourceCancel;

        IEventSubscription eventSubscription;

        public bool Available { get { return stage == Stage.Available; } }

        public event EventHandler AvailableChanged;

        void RaiseAvailableChanged()
        {
            if (AvailableChanged != null)
                AvailableChanged(this, EventArgs.Empty);
        }

        public async Task ActivateAsync()
        {
            if (Active)
                return;

            if (connection == null)
                throw new InvalidOperationException("Connection property must be set");

            Active = true;

            if (connection.ConnectionState == ConnectionState.Open)
                await InitializeAsync(automatic: false);
        }

        public async Task DeactivateAsync()
        {
            if (stage == Stage.Available)
            {
                stage = Stage.Idle;
                RaiseAvailableChanged();
                Dispose(true);
            }
            else if (stage == Stage.GettingResource)
            {
                gettingResourceCancel.Cancel();
            }
        }

        void connection_ConnectionStateChanged(object sender, EventArgs e)
        {
            if (Connection.ConnectionState == ConnectionState.Open && Active)
                InitializeAsync(automatic: true);

            if (Connection.ConnectionState == ConnectionState.Close)
                DeactivateAsync();
        }

        public event EventHandler<InternalExceptionThrownEventArgs> InternalExceptionThrown;

        async Task InitializeAsync(bool automatic)
        {
            if (stage == Stage.Idle)
            {
                eventSubscription = Connection.EventReceiver.SubscribeAll(HandleEvent);

                stage = Stage.GettingResource;

                gettingResourceCancel = new CancellationTokenSource();
                IGenesysResponse<UserResourceResponse> response = null;
                try
                {
                    // doc: http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together
                    response = await Connection.Client.CreateRequest("GET", "/api/v2/me?subresources=*").SendAsync<UserResourceResponse>(gettingResourceCancel.Token);
                }
                catch (OperationCanceledException)
                {
                    stage = Stage.Idle;
                    Dispose(true);
                }
                catch (Exception e)
                {
                    stage = Stage.Idle;

                    if (automatic && InternalExceptionThrown != null)
                        InternalExceptionThrown(this, new InternalExceptionThrownEventArgs(e));

                    Dispose(true);

                    if (!automatic)
                        throw;
                }

                if (response != null)
                {
                    var userResource = (IDictionary<string, object>)response.AsDictionary["user"];
                    var untypedSettings = (IDictionary<string, object>)userResource["settings"];
                    Settings = untypedSettings.ToDictionary(kvp => kvp.Key, kvp => (IDictionary<string, object>)kvp.Value);

                    stage = Stage.Available;
                    RaiseAvailableChanged();

                    RaiseResourceUpdated();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                try
                {
                    eventSubscription.Dispose();
                    eventSubscription = null;
                }
                catch (Exception e)
                {
                    InternalExceptionThrown(this, new InternalExceptionThrownEventArgs(e));
                }

            base.Dispose(disposing);
        }

        public Task ChangeState(string value)
        {
            return Connection.Client.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        public event Action<UserResource> ResourceUpdatedInternal;
        public event Action<GenesysEvent> GenesysEventReceivedInternal;
        public event EventHandler<EventArgs> ResourceUpdated;

        void HandleEvent(object sender, GenesysEvent e)
        {
            var user = e.GetResourceAsTypeOrNull<IDictionary<string, object>>("user");

            if (GenesysEventReceivedInternal != null)
                GenesysEventReceivedInternal(e);
        }

        // TODO: include event or resource info
        void RaiseResourceUpdated()
        {
            if (ResourceUpdated != null)
                ResourceUpdated(this, EventArgs.Empty);
        }
        
        public IDictionary<string, IDictionary<string, object>> Settings { get; private set; }
    }
}
