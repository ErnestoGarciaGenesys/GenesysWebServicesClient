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

        GenesysUser user;

        [Category("Activation")]
        public GenesysUser User
        {
            get { return user; }
            set
            {
                if (user != null && user != value)
                    throw new InvalidOperationException("User can only be set once");

                user = value;
                value.InternalUpdated += User_InternalUpdated;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (User != null)
                {
                    User.InternalUpdated -= User_InternalUpdated;
                }

         	base.Dispose(disposing);
        }

        void User_InternalUpdated(object sender, InternalUpdatedEventArgs e)
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
                    if (Calls.Count == 1)
                    {
                        call.PropertyChanged += firstCall_PropertyChanged;
                        firstCall_PropertyChanged(call, new PropertyChangedEventArgs("Capabilities"));
                    }
                }
                else
                {
                    call.HandleEvent(e.GenesysEvent.NotificationType, callResource);
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
