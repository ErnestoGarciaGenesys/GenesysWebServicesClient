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
        readonly IList<CallOperation> operations = new List<CallOperation>();

        public string Id { get; private set; }
        public string State { get; private set; }
        
        IList<string> capabilities;
        IReadOnlyCollection<string> readOnlyCapabilities;

        public CallUserData UserData { get { return userData; } }

        class CallOperation : NotifyPropertyChangedSupport, IOperation
        {
            readonly GenesysCall call;
            readonly string operationName;

            public bool IsCapable { get; private set; }

            public CallOperation(GenesysCall call, string operationName)
            {
                this.call = call;
                this.operationName = operationName;
                call.operations.Add(this);
            }

            public void Do()
            {
                call.callManager.User.Connection.Client.CreateRequest(
                    "POST", "/api/v2/me/calls/" + call.Id,
                    new { operationName = operationName })
                        .SendAsync();
            }

            internal void HandleEvent(IList<string> capabilities)
            {
                ChangeAndNotifyProperty("IsCapable", capabilities.Contains(operationName));
            }
        }

        public IOperation AnswerOperation { get; private set; }
        public IOperation HangupOperation { get; private set; }

        public GenesysCall(GenesysCallManager callManager, CallResource callResource)
        {
            this.callManager = callManager;
            Id = callResource.id;
            State = callResource.state;
            SetCapabilities(callResource.capabilities);
            userData = new CallUserData(callResource);
            AnswerOperation = new CallOperation(this, "Answer");
            HangupOperation = new CallOperation(this, "Hangup");
            foreach (var op in operations)
                op.HandleEvent(callResource.capabilities);
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
                SetCapabilities(callResource.capabilities);
                foreach (var op in operations)
                    op.HandleEvent(callResource.capabilities);
                RaisePropertyChanged("Capabilities");
            }
        }
        
        public bool Finished
        {
            get { return State == "Released"; }
        }

        public IReadOnlyCollection<string> Capabilities
        {
            get { return readOnlyCapabilities; }
        }
    }
}
