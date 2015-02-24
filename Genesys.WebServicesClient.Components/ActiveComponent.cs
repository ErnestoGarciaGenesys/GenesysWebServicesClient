using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    [Obsolete]
    public abstract class ActiveComponent : DisposableComponent
    {
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
    }
}
