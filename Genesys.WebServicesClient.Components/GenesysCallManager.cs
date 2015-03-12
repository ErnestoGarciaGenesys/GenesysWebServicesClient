using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysCallManager : GenesysComponent
    {
        readonly BindingList<GenesysCall> calls = new BindingList<GenesysCall>();
        public BindingList<GenesysCall> Calls { get { return calls; } }

        [Category("Activation")]
        public GenesysUser User
        {
            get { return (GenesysUser)Parent; }
            set
            {
                if (Parent != null && Parent != value)
                    throw new InvalidOperationException("User can only be set once");

                Parent = value;
            }
        }

        protected override void OnParentUpdated(InternalUpdatedEventArgs e)
        {
            if (e.GenesysEvent != null && e.GenesysEvent.MessageType == "CallStateChangeMessage")
            {
                var callResource = e.GenesysEvent.GetResourceAsType<CallResource>("call");
                var call = Calls.FirstOrDefault(c => c.Id == callResource.id);
                bool newCall = call == null;
                if (newCall)
                {
                    call = new GenesysCall(this, callResource);
                    Calls.Add(call);
                }
                else
                {
                    call.HandleEvent(e.DelayedEvents, e.GenesysEvent.NotificationType, callResource);
                }

                if (call.Finished)
                {
                    Calls.Remove(call);
                }

                // Quick temporary solution: always notify changes on ActiveCall
                RaisePropertyChanged(e.DelayedEvents, "ActiveCall");
            }
        }

        public GenesysCall ActiveCall { get { return calls.FirstOrDefault(); } }
    }
}
