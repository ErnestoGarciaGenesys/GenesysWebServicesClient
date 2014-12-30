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
        GenesysUser user;

        readonly BindingList<GenesysCall> calls = new BindingList<GenesysCall>();
        public BindingList<GenesysCall> Calls { get { return calls; } }

        protected override void ActivateImpl()
        {
            User.CallManager = this;
        }

        protected override void DeactivateImpl()
        {
            User.CallManager = null;
        }

        public GenesysUser User
        {
            get { return user; }
            set
            {
                user = value;
                parentComponent = value;
            }
        }

        internal void HandleEvent(GenesysEvent e)
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

        public void Answer()
        {
            if (Calls.Count > 0)
            {
                User.Connection.Client.CreateRequest("POST", "/api/v2/me/calls/" + Calls[0].Id, new { operationName = "Answer" }).SendAsync();
            }
        }

        public bool AnswerCapable
        {
            get { return Calls.Count > 0 && Calls[0].Capabilities.Contains("Answer"); }
        }

        public void Hangup()
        {
            if (Calls.Count > 0)
            {
                User.Connection.Client.CreateRequest("POST", "/api/v2/me/calls/" + Calls[0].Id, new { operationName = "Hangup" }).SendAsync();
            }
        }

        public bool HangupCapable
        {
            get { return Calls.Count > 0 && Calls[0].Capabilities.Contains("Hangup"); }
        }

        public void InitiateTransfer(string phoneNumber)
        {
            if (Calls.Count > 0)
            {
                User.Connection.Client.CreateRequest("POST", "/api/v2/me/calls/" + Calls[0].Id,
                    new
                    {
                        operationName = "InitiateTransfer",
                        destination = new { phoneNumber = phoneNumber }
                    }
                ).SendAsync();
            }
        }

        public bool InitiateTransferCapable
        {
            get { return Calls.Count > 0 && Calls[0].Capabilities.Contains("InitiateTransfer"); }
        }

        public void CompleteTransfer()
        {
            if (Calls.Count > 0)
            {
                User.Connection.Client.CreateRequest("POST", "/api/v2/me/calls/" + Calls[0].Id, new { operationName = "CompleteTransfer" }).SendAsync();
            }
        }

        public bool CompleteTransferCapable
        {
            get { return Calls.Count > 0 && Calls[0].Capabilities.Contains("CompleteTransfer"); }
        }
    }
}
