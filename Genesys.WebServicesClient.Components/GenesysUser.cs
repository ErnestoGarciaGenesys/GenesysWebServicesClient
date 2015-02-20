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
    public class GenesysUser : NotifyPropertyChangedComponent
    {
        /// <summary>
        /// That this object is active means that it is being monitored and will be recovered in the case of reconnections.
        /// </summary>
        public bool Active { get; private set; }

        GenesysConnection connection;

        enum Stage { Idle, GettingResource, Available }

        Stage stage = Stage.Idle;

        // Valid only during the GettingResource stage
        CancellationTokenSource gettingResourceCancel;

        // Needs disposal
        IEventSubscription eventSubscription;

        /// <summary>
        /// That this object is available means that this object has all its resource properties and methods available to use.
        /// Notice that Available implies <see cref="Active"/>, but Active does not imply Available.
        /// </summary>
        public bool Available { get { return stage == Stage.Available; } }

        public event EventHandler AvailableChanged;

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

        public async Task ActivateAsync()
        {
            if (Active)
                return;

            if (connection == null)
                throw new InvalidOperationException("Connection property must be set");


            Active = true;

            if (connection.ConnectionState == ConnectionState.Open)
                await InitializeAsync(recovering: false);
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
                InitializeAsync(recovering: true);

            if (Connection.ConnectionState == ConnectionState.Close)
                DeactivateAsync();
        }

        /// <summary>
        /// Raised when an automatic recovery (re-activation) of this resource failed.
        /// </summary>
        public event EventHandler<RecoveryFailedEventArgs> RecoveryFailed;
        
        void RaiseRecoveryFailed(ActivationException e)
        {
            if (RecoveryFailed != null)
                RecoveryFailed(this, new RecoveryFailedEventArgs(e));
        }

        async Task InitializeAsync(bool recovering)
        {
            if (stage == Stage.Idle)
            {
                eventSubscription = Connection.EventReceiver.SubscribeAll(HandleEvent);

                stage = Stage.GettingResource;

                // Documentation about recovering existing state:
                // http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together

                gettingResourceCancel = new CancellationTokenSource();
                IGenesysResponse<UserResourceResponse> response = null;
                var request = Connection.Client.CreateRequest("GET", "/api/v2/me?subresources=*");
                
                try
                {
                    response = await request.SendAsync<UserResourceResponse>(gettingResourceCancel.Token);
                }
                catch (OperationCanceledException)
                {
                    stage = Stage.Idle;
                    Dispose(true);
                }
                catch (Exception e)
                {
                    stage = Stage.Idle;

                    if (recovering)
                        RaiseRecoveryFailed(new ActivationException(e));

                    Dispose(true);

                    if (!recovering)
                        throw;
                }

                if (response != null)
                {
                    var userResource = (IDictionary<string, object>)response.AsDictionary["user"];
                    var untypedSettings = (IDictionary<string, object>)userResource["settings"];
                    
                    // Concretizing dictionary type to a dictionary of dictionaries,
                    // because Settings contains sections, which contain key-value pairs.
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

            base.Dispose(disposing);
        }

        void RaiseAvailableChanged()
        {
            if (AvailableChanged != null)
                AvailableChanged(this, EventArgs.Empty);
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

    public class ActivationException : Exception
    {
        public ActivationException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }

    public class RecoveryFailedEventArgs : EventArgs
    {
        private readonly ActivationException exception;

        public RecoveryFailedEventArgs(ActivationException exception)
        {
            this.exception = exception;
        }

        public ActivationException ActivationException
        {
            get { return exception; }
        }
    }

}
