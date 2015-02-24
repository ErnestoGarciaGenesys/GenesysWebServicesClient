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
        GenesysConnection connection;

        bool autoRecover = false;

        enum Stage { Idle, Recovering, Available }

        Stage stage = Stage.Idle;

        // Use only during the Recovering stage
        CancellationTokenSource recoveringCancelToken;

        readonly AwaitingActivate awaitingActivate = new AwaitingActivate();

        // Needs disposal
        IEventSubscription eventSubscription;

        /// <summary>
        /// Available means that this object has been correctly initialized and all its
        /// resource properties and methods are available to use.
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
                    if (stage != Stage.Idle)
                        throw new InvalidOperationException("Connection can't be changed while this object is active");

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

        /// <summary>
        /// Loads current object state and subscribes for its events, in order to keep up-to-date.
        /// This method only triggers the activation procedure, it does not wait for completion, nor
        /// guarantees a succesful activation.
        /// After calling this method, this object will be automatically recovered on reconnections
        /// (when the connection is lost and then open again).
        /// In order to wait for completion, please use <see cref="ActivateAsync()"/>.
        /// </summary>
        public void Activate()
        {
            autoRecover = true;

            if (stage == Stage.Idle && connection.ConnectionState == ConnectionState.Open)
            {
                var dummy = RecoverAsync(background: true); // assignment is for avoiding warning for not using await
            }
        }

        /// <summary>
        /// Loads current object state and subscribes for its events, in order to keep up-to-date.
        /// This method waits for completion of the activation procedure, and will throw an exception if
        /// activation fails for some reason.
        /// If this method completes successfully, this object will be automatically recovered on reconnections
        /// (when the connection is lost and then open again). If the method fails, then automatic recovery will
        /// not be enabled.
        /// </summary>
        /// <exception cref="ActivationException">If object activation failed.</exception>
        public async Task ActivateAsync()
        {
            if (stage == Stage.Idle)
            {
                if (connection == null)
                    throw new InvalidOperationException("Connection property must be set");

                if (connection.ConnectionState != ConnectionState.Open)
                    throw new ActivationException("Connection is not open");

                await RecoverAsync(background: false);
            }
            else if (stage == Stage.Recovering)
            {
                // Recovering is ongoing. Wait for completion.
                await awaitingActivate.Await();
            }
            else if (stage == Stage.Available)
            {
                // Do nothing
            }
        }

        async Task RecoverAsync(bool background)
        {
            stage = Stage.Recovering;

            try
            {
                await RecoverImpl();
                autoRecover = true;
                stage = Stage.Available;
                RaiseAvailableChanged();
                awaitingActivate.Complete(null);
            }
            catch (Exception e)
            {
                stage = Stage.Idle;
                Dispose(true);
                awaitingActivate.Complete(e);

                if (background)
                    RaiseRecoveryFailed(new ActivationException(e));
                else
                    throw;
            }
        }

        public void Deactivate()
        {
            if (stage == Stage.Available)
            {
                stage = Stage.Idle;
                RaiseAvailableChanged();
                Dispose(true);
            }
            else if (stage == Stage.Recovering)
            {
                recoveringCancelToken.Cancel();
            }
        }

        void connection_ConnectionStateChanged(object sender, EventArgs e)
        {
            if (Connection.ConnectionState == ConnectionState.Open && autoRecover)
                Activate();

            if (Connection.ConnectionState == ConnectionState.Close)
                Deactivate();
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

        async Task RecoverImpl()
        {
            eventSubscription = Connection.EventReceiver.SubscribeAll(HandleEvent);

            // Documentation about recovering existing state:
            // http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together

            recoveringCancelToken = new CancellationTokenSource();
            var request = Connection.Client.CreateRequest("GET", "/api/v2/me?subresources=*");
            var response = await request.SendAsync<UserResourceResponse>(recoveringCancelToken.Token);
            LoadResource(response);
            RaiseResourceUpdated();
        }

        void LoadResource(IGenesysResponse<UserResourceResponse> response)
        {
            var userResource = (IDictionary<string, object>)response.AsDictionary["user"];
            var untypedSettings = (IDictionary<string, object>)userResource["settings"];

            // Concretizing dictionary type to a dictionary of dictionaries,
            // because Settings contains sections, which contain key-value pairs.
            Settings = untypedSettings.ToDictionary(kvp => kvp.Key, kvp => (IDictionary<string, object>)kvp.Value);
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

        public ActivationException(string message)
            : base(message)
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

    class AwaitingActivate
    {
        readonly IList<TaskCompletionSource<object>> completionSources = new List<TaskCompletionSource<object>>();

        internal async Task Await()
        {
            var c = new TaskCompletionSource<object>();
            completionSources.Add(c);
            await c.Task;
        }

        internal void Complete(Exception exc)
        {
            foreach (var c in completionSources)
            {
                if (exc == null)
                    c.SetResult(null);
                else if (exc is OperationCanceledException)
                    c.SetCanceled();
                else
                    c.SetException(exc);
            }

            completionSources.Clear();
        }
    }

}
