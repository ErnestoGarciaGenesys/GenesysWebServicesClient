using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysChat : GenesysInteraction
    {
        public string ChatType { get; private set; }

        readonly BindingList<GenesysChatMessage> messages = new BindingList<GenesysChatMessage>();
        public BindingList<GenesysChatMessage> Messages { get { return messages; } }

        public GenesysChat(GenesysInteractionManager interactionManager, ChatResource chatResource) :
            base(interactionManager, chatResource)
        {
            ChatType = chatResource.chatType;
        }

        public override bool Finished
        {
            get { return State == "Completed" || State == "Revoked"; }
        }


        internal void HandleMessageLogUpdatedEvent(INotifications notifications, GenesysEvent genesysEvent)
        {
            if (genesysEvent.NotificationType == "NewMessages")
            {
                var newMessages = genesysEvent.GetResourceAsType<IReadOnlyList<MessageResource>>("messages");
                
                if (newMessages.Count > 0)
                {
                    var maxIndex = newMessages.Max(m => m.index);
                    foreach (var m in newMessages)
                        //messages.Insert(m.index - 1, m); // doesn't work, sometimes non-contiguous message indices are received
                        messages.Add(new GenesysChatMessage(m));
                }
            }
        }

        void DoChatOperation(object parameters)
        {
            interactionManager.User.Connection.InternalClient.CreateRequest("POST", "/api/v2/me/chats/" + Id, parameters).SendAsync();
        }

        void DoChatOperation(string operationName)
        {
            DoChatOperation(new { operationName = operationName });
        }

        protected override string[] GetCapabilityNames()
        {
            return new[] {
                "Accept",
                "Reject",
                "Complete",

                "Leave",
                "SendMessage",

                //"Transfer",
                //"Invite",
                //"Consult",
                //"SetInFocus",
                //"SetDisposition",
                //"AttachUserData",
                //"DeleteUserData",
                //"UpdateUserData",
                //"Complete",
                //"SendUrl",
                //"SendStartTypingNotification",
                //"SendStopTypingNotification",
            };
        }

        public void Accept(string nickname)
        {
            DoChatOperation(
                new {
                    operationName = "Accept",
                    nickname = nickname,
                });
        }

        public bool AcceptCapable { get; private set; }

        public void Reject() { DoChatOperation("Reject"); }
        public bool RejectCapable { get; private set; }

        public void Complete() { DoChatOperation("Complete"); }
        public bool CompleteCapable { get; private set; }

        public void SendMessage(string text)
        {
            DoChatOperation(
                new
                {
                    operationName = "SendMessage",
                    text = text,
                });
        }

        public bool SendMessageCapable { get; private set; }

        public void Leave() { DoChatOperation("Leave"); }
        public bool LeaveCapable { get; private set; }
    }
}
