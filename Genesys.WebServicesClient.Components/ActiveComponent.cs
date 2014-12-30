using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public abstract class ActiveComponent : DisposableComponent, ISupportInitialize
    {
        bool autoActivate = true;
        
        protected ActiveComponent parentComponent;
        
        protected bool isParent = false;

        [Browsable(false)]
        public bool Active
        {
            get;
            private set;
        }

        public event EventHandler ActiveChanged;

        [DefaultValue(true)]
        public virtual bool AutoActivate
        {
            get { return autoActivate; }
            set { autoActivate = value; }
        }

        void ISupportInitialize.BeginInit() { }

        void ISupportInitialize.EndInit()
        {
            if (!isParent && parentComponent != null)
            {
                parentComponent.ActiveChanged += parentComponent_ActiveChanged;

                if (autoActivate && parentComponent.Active)
                    Activate();
            }
        }

        void parentComponent_ActiveChanged(object sender, EventArgs e)
        {
            if (parentComponent.Active)
            {
                if (autoActivate)
                    Activate();
            }
            else
            {
                if (autoActivate)
                    Deactivate();
            }
        }

        public void Activate()
        {
            if (!isParent && parentComponent == null)
                throw new InvalidOperationException("parent component not set");

            if (Active)
                return;

            if (!isParent)
                parentComponent.Activate();

            ActivateImpl();

            Active = true;
            RaiseActiveChanged();
        }

        public void Deactivate()
        {
            if (!Active)
                return;

            DeactivateImpl();

            Active = false;
            RaiseActiveChanged();
        }

        void RaiseActiveChanged()
        {
            if (ActiveChanged != null)
                ActiveChanged(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Deactivate();

            base.Dispose(disposing);
        }

        protected abstract void ActivateImpl();

        protected abstract void DeactivateImpl();
    }
}
