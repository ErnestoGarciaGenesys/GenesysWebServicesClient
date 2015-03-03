using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public abstract class ActiveGenesysComponent : GenesysComponent
    {
        readonly AwaitingStart awaitingStart = new AwaitingStart();

        protected ActivationStage activationStage = ActivationStage.Idle;

        protected bool autoRecover = false;

        // Valid only during the Starting stage
        CancellationTokenSource startCancelToken;

        /// <summary>
        /// This method only triggers the activation procedure, it does not wait for completion, nor
        /// guarantees a succesful activation.
        /// After calling this method, this object will be automatically recovered.
        /// In order to wait for completion, please use <see cref="StartAsync()"/>.
        /// </summary>
        public void Start()
        {
            autoRecover = true;

            if (activationStage == ActivationStage.Idle && CanStart() != null)
            {
                var _ = DoStartAsync(background: true); // assigment to prevent warning for not using await
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
        public async Task StartAsync()
        {
            if (activationStage == ActivationStage.Idle)
            {
                var exc = CanStart();
                if (exc != null)
                    throw exc;

                await DoStartAsync(background: false);
            }
            else if (activationStage == ActivationStage.Starting)
            {
                // Start is ongoing. Await completion
                await awaitingStart.Await();
            }
            else
            {
                // Already started. Nothing to do
            }
        }

        async Task DoStartAsync(bool background)
        {
            try
            {
                SetActivationStage(ActivationStage.Starting);
                startCancelToken = new CancellationTokenSource();
                await StartImplAsync(startCancelToken.Token);
                autoRecover = true;
                SetActivationStage(ActivationStage.Started);
                awaitingStart.Complete(null);
            }
            catch (Exception e)
            {
                StopImpl();
                SetActivationStage(ActivationStage.Idle);
                awaitingStart.Complete(e);

                if (background)
                    RaiseRecoveryFailed(new ActivationException(e));
                else
                    throw;
            }
        }

        public void Stop()
        {
            if (activationStage == ActivationStage.Started)
            {
                StopImpl();
                SetActivationStage(ActivationStage.Idle);
            }
            else if (activationStage == ActivationStage.Starting)
            {
                startCancelToken.Cancel();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Stop();

            base.Dispose(disposing);
        }

        protected abstract Task StartImplAsync(CancellationToken cancellationToken);

        protected virtual Exception CanStart()
        {
            return null;
        }

        /// <summary>
        /// Implementations must not throw exceptions.
        /// </summary>
        protected abstract void StopImpl();

        void SetActivationStage(ActivationStage s)
        {
            if (activationStage != s)
            {
                activationStage = s;
                OnActivationStageChanged();
            }
        }

        [Browsable(false)]
        public ActivationStage InternalActivationStage
        {
            get { return activationStage; }
        }

        public event EventHandler InternalActivationStageChanged;

        protected virtual void OnActivationStageChanged()
        {
            if (InternalActivationStageChanged != null)
                InternalActivationStageChanged(this, EventArgs.Empty);
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

        ////protected bool isParent = false;
        //ActiveComponent parentComponent;

        //protected ActiveComponent ParentComponent
        //{
        //    get { return parentComponent; }
        //    set
        //    {
        //        parentComponent = value;

        //        //if (parentComponent == null)
        //        //    parentComponent.AvailableChanged -= parentComponent_AvailableChanged;
        //        //else
        //        //    parentComponent.AvailableChanged += parentComponent_AvailableChanged;
        //    }
        //}

        //[Obsolete]
        //bool autoActivate = true;

        ///// <summary>
        ///// If set to true, this component will be activated automatically right after its parent is activated.
        ///// in that case there is no need to call <see cref="Initialize()"/> on this component.
        ///// </summary>
        //[Obsolete, DefaultValue(true), Category("Activation")]
        //public virtual bool AutoActivate
        //{
        //    get { return autoActivate; }
        //    set { autoActivate = value; }
        //}

        //void parentComponent_AvailableChanged(object sender, EventArgs e)
        //{
        //    if (parentComponent.Available)
        //    {
        //        if (Started)
        //            Initialize();
        //    }
        //    else
        //    {
        //        if (Available)
        //            Dispose(true);
        //    }
        //}

        //void Initialize()
        //{
        //    if (Available)
        //        return;

        //    if (!isParent && parentComponent == null)
        //        throw new InvalidOperationException("parent component not set");

        //    if (!isParent)
        //        parentComponent.Initialize();

        //    ActivateImpl();

        //    Available = true;
        //    RaiseActiveChanged();
        //}

        //public void Deactivate()
        //{
        //    if (!Available)
        //        return;

        //    DeactivateImpl();
            
        //    Available = false;
        //    RaiseActiveChanged();
        //}

        //void RaiseActiveChanged()
        //{
        //    if (AvailableChanged != null)
        //        AvailableChanged(this, EventArgs.Empty);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //        Deactivate();

        //    base.Dispose(disposing);
        //}

        //protected abstract void ActivateImpl();

        //protected abstract void DeactivateImpl();

        class AwaitingStart
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

    public enum ActivationStage
    {
        Idle,
        Starting,
        Started
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
}
