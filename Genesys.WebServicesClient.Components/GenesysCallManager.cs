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
    public class GenesysCallManager : NotifyPropertyChangedComponent
    {
        readonly BindingList<GenesysCall> calls = new BindingList<GenesysCall>();
        public BindingList<GenesysCall> Calls { get { return calls; } }

        [Category("Activation")]
        public GenesysUser User
        {
            get { return (GenesysUser)ParentComponent; }
            set
            {
                if (ParentComponent != null && ParentComponent != value)
                    throw new InvalidOperationException("User can only be set once");

                ParentComponent = value;
                value.ResourceUpdatedInternal += User_ResourceUpdatedInternal;
                value.GenesysEventReceivedInternal += User_GenesysEventReceivedInternal;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (User != null)
                {
                    User.ResourceUpdatedInternal -= User_ResourceUpdatedInternal;
                    User.GenesysEventReceivedInternal -= User_GenesysEventReceivedInternal;
                }

         	base.Dispose(disposing);
        }

        void User_ResourceUpdatedInternal(UserResource user)
        {
            // TODO
        }

        void User_GenesysEventReceivedInternal(GenesysEvent e)
        {
            if (e.MessageType == "CallStateChangeMessage")
            {
                var callResource = e.GetResourceAsType<CallResource>("call");
                var call = Calls.FirstOrDefault(c => c.Id == callResource.id);
                bool newCall = call == null;
                if (newCall)
                {
                    call = new GenesysCall(this, callResource);
                    Calls.Add(call);
                    if (Calls.Count == 1)
                    {
                        call.PropertyChanged += firstCall_PropertyChanged;
                        firstCall_PropertyChanged(call, new PropertyChangedEventArgs("Capabilities"));
                    }
                }
                else
                {
                    call.HandleEvent(e.NotificationType, callResource);
                }

                if (call.Finished)
                {
                    Calls.Remove(call);
                    call.PropertyChanged -= firstCall_PropertyChanged;
                }

                // Quick temporary solution: always notify changes on ActiveCall
                RaisePropertyChanged("ActiveCall");
            }
        }

        void firstCall_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Capabilities")
                RaisePropertyChanged("AnswerCallCapable");
        }

        public GenesysCall ActiveCall { get { return calls.FirstOrDefault(); } }

    }
}
