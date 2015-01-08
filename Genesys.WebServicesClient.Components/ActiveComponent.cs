using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public abstract class ActiveComponent : DisposableComponent
    {
        protected bool isParent = false;

        [Browsable(false)]
        public bool Active
        {
            get;
            private set;
        }

        public event EventHandler ActiveChanged;

        ActiveComponent parentComponent;

        protected ActiveComponent ParentComponent
        {
            get { return parentComponent; }
            set
            {
                parentComponent = value;

                if (parentComponent == null)
                    parentComponent.ActiveChanged -= parentComponent_ActiveChanged;
                else
                    parentComponent.ActiveChanged += parentComponent_ActiveChanged;
            }
        }

        bool autoActivate = true;

        /// <summary>
        /// If set to true, this component will be activated automatically right after its parent is activated.
        /// in that case there is no need to call <see cref="Activate()"/> on this component.
        /// </summary>
        [DefaultValue(true), Category("Activation")]
        public virtual bool AutoActivate
        {
            get { return autoActivate; }
            set { autoActivate = value; }
        }

        void parentComponent_ActiveChanged(object sender, EventArgs e)
        {
            if (parentComponent.Active)
            {
                if (AutoActivate)
                    Activate();
            }
            else
            {
                if (AutoActivate)
                    Deactivate();
            }
        }

        public void Activate()
        {
            if (Active)
                return;

            if (!isParent && parentComponent == null)
                throw new InvalidOperationException("parent component not set");

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
