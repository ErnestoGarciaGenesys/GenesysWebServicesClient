using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysCall : NotifyPropertyChangedSupport
    {
        readonly GenesysCallManager callManager;
        readonly CallUserData userData;

        public string Id { get; private set; }
        public string State { get; private set; }
        
        IList<string> capabilities;
        IReadOnlyCollection<string> readOnlyCapabilities;

        public CallUserData UserData { get { return userData; } }

        public GenesysCall(GenesysCallManager callManager, CallResource callResource)
        {
            this.callManager = callManager;
            Id = callResource.id;
            State = callResource.state;
            Participants = callResource.participants;
            SetCapabilities(callResource.capabilities);
            userData = new CallUserData(callResource);
            UpdateCapableProperties(callResource.capabilities);
        }

        public bool Finished
        {
            get { return State == "Released"; }
        }

        public IReadOnlyCollection<string> Capabilities
        {
            get { return readOnlyCapabilities; }
        }

        void SetCapabilities(IList<string> value)
        {
            capabilities = value;
            readOnlyCapabilities = new ReadOnlyCollection<string>(capabilities);
        }

        internal void HandleEvent(string notificationType, CallResource callResource)
        {
            bool userDataChanged = userData.HandleEvent(notificationType, callResource);
            if (userDataChanged)
                RaisePropertyChanged("UserData");

            if (notificationType == "StatusChange")
            {
                ChangeAndNotifyProperty("State", callResource.state);
                ChangeAndNotifyProperty("Participants", callResource.participants);
                SetCapabilities(callResource.capabilities);
                UpdateCapableProperties(callResource.capabilities);
                RaisePropertyChanged("Capabilities");
            }
        }

        void DoCallOperation(object parameters)
        {
            callManager.User.Connection.Client.CreateRequest("POST", "/api/v2/me/calls/" + Id, parameters).SendAsync();
        }

        void DoCallOperation(string operationName)
        {
            DoCallOperation(new { operationName = operationName });
        }

        void UpdateCapableProperties(IList<string> capabilities)
        {
            foreach (var op in callOperations)
                ChangeAndNotifyProperty(op + "Capable", capabilities.Contains(op));
        }

        readonly string[] callOperations =
        {
            "Answer",
            "Hangup",
            "InitiateTransfer",
            "CompleteTransfer",
            "UpdateUserData",
        };

        public void Answer() { DoCallOperation("Answer"); }
        public bool AnswerCapable { get; private set; }

        public void Hangup() { DoCallOperation("Hangup"); }
        public bool HangupCapable { get; private set; }

        public void InitiateTransfer(string phoneNumber)
        {
            DoCallOperation(new
                {
                    operationName = "InitiateTransfer",
                    destination = new { phoneNumber = phoneNumber }
                });
        }

        public bool InitiateTransferCapable { get; private set; }

        public void CompleteTransfer() { DoCallOperation("CompleteTransfer"); }
        public bool CompleteTransferCapable { get; private set; }

        public void UpdateUserData(object obj)
        {
            DoCallOperation(new
            {
                operationName = "UpdateUserData",
                userData = obj
            });
        }

        public bool UpdateUserDataCapable { get; private set; }

        public IList<string> Participants { get; private set; }
    }
}
