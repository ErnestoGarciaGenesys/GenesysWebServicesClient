using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public abstract class AutoInitComponent : DisposableComponent, ISupportInitializeNotification
    {
        bool autoInitialize = true;
        
        protected AutoInitComponent parentComponent;
        
        protected bool isParent = false;

        [Browsable(false)]
        public bool IsInitialized
        {
            get;
            private set;
        }

        public event EventHandler Initialized;

        [DefaultValue(true)]
        public virtual bool AutoInitialize
        {
            get { return autoInitialize; }
            set { autoInitialize = value; }
        }

        void ISupportInitialize.BeginInit() { }

        void ISupportInitialize.EndInit()
        {
            if (!isParent && parentComponent != null)
            {
                parentComponent.Initialized += parentComponent_Initialized;
                parentComponent.Disposed += parentComponent_Disposed;

                if (autoInitialize && ((ISupportInitializeNotification)parentComponent).IsInitialized)
                    Initialize();
            }
        }

        void parentComponent_Initialized(object sender, EventArgs e)
        {
            if (autoInitialize)
                Initialize();
        }

        void parentComponent_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void Initialize()
        {
            if (!isParent && parentComponent == null)
                throw new InvalidOperationException("parent component not set");

            if (IsInitialized)
                return;

            InitializeImpl();

            IsInitialized = true;

            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            IsInitialized = false;
            base.Dispose(disposing);
        }

        protected abstract void InitializeImpl();
    }
}
