using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysCall : GenesysInteraction
    {
        public GenesysCall(GenesysInteractionManager interactionManager, CallResource callResource) :
            base(interactionManager, callResource)
        {
        }

        public override bool Finished
        {
            get { return State == "Released"; }
        }

        void DoCallOperation(object parameters)
        {
            interactionManager.User.Connection.InternalClient.CreateRequest("POST", "/api/v2/me/calls/" + Id, parameters).SendAsync();
        }

        void DoCallOperation(string operationName)
        {
            DoCallOperation(new { operationName = operationName });
        }

        protected override string[] GetCapabilityNames()
        {
            return new[] {
                "Answer",
                "Hangup",
                "InitiateTransfer",
                "CompleteTransfer",
                "AttachUserData",
                "UpdateUserData",
            };
        }

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

        public void AttachUserData(object obj)
        {
            DoCallOperation(new
            {
                operationName = "AttachUserData",
                userData = obj
            });
        }

        public bool AttachUserDataCapable { get; private set; }

        public void UpdateUserData(object obj)
        {
            DoCallOperation(new
            {
                operationName = "UpdateUserData",
                userData = obj
            });
        }

        public bool UpdateUserDataCapable { get; private set; }
    }
}
