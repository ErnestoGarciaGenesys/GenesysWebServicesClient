using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Components
{
    /// <summary>
    /// 
    /// <para>
    /// Observable properties of GenesysComponents are typically read-only. Mark them <c>[Readonly(true)]</c>.
    /// Do not mark them <c>[Browsable(false)]</c>, as Windows Forms will not allow binding to non-browsable properties.
    /// </para>
    /// </summary>
    public class GenesysComponent : DisposableComponent
    {
        GenesysComponent parent;

        event EventHandler<InternalUpdatedEventArgs> InternalUpdated;

        protected GenesysComponent Parent
        {
            get
            {
                return parent;
            }

            set
            {
                if (value != parent)
                {
                    if (value == null)
                    {
                        // null value must be allowed, as it is typical to be set by Visual Designer
                        parent.InternalUpdated -= parent_InternalUpdated;
                        parent = null;
                    }
                    else
                    {
                        parent = value;
                        parent.InternalUpdated += parent_InternalUpdated;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Parent != null)
                    Parent.InternalUpdated -= parent_InternalUpdated;
            }

            base.Dispose(disposing);
        }

        void parent_InternalUpdated(object sender, InternalUpdatedEventArgs e)
        {
            OnParentUpdated(e);
        }

        protected virtual void OnParentUpdated(InternalUpdatedEventArgs e)
        {
        }

        protected void RaiseInternalUpdated(InternalUpdatedEventArgs e)
        {
            if (InternalUpdated != null)
                InternalUpdated(this, e);
        }

        protected void StartHierarchyUpdate(GenesysEvent genesysEvent)
        {
            var postEvents = new PostEventsImpl();
            var eventArgs = new InternalUpdatedEventArgs(postEvents, genesysEvent);
            OnStartHierarchyUpdate(eventArgs);
            RaiseInternalUpdated(eventArgs);
            postEvents.Run();
        }

        protected void StartHierarchyUpdate()
        {
            StartHierarchyUpdate(null);
        }

        protected virtual void OnStartHierarchyUpdate(InternalUpdatedEventArgs e)
        {
        }

        public class InternalUpdatedEventArgs : EventArgs
        {
            public GenesysEvent GenesysEvent { get; private set; }

            public IPostEvents PostEvents { get; private set; }

            public InternalUpdatedEventArgs(IPostEvents postEvents)
            {
                PostEvents = postEvents;
            }

            public InternalUpdatedEventArgs(IPostEvents postEvents, GenesysEvent e)
                : this(postEvents)
            {
                GenesysEvent = e;
            }
        }

        class PostEventsImpl : IPostEvents
        {
            List<Action> list = new List<Action>();

            public void Add(Action a)
            {
                list.Add(a);
            }

            public void Run()
            {
                foreach (var a in list)
                    a();
            }
        }
    }
}
