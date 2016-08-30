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
    public class GenesysInteractionManager : GenesysComponent
    {
        readonly BindingList<GenesysCall> calls = new BindingList<GenesysCall>();
        public BindingList<GenesysCall> Calls { get { return calls; } }

        readonly BindingList<GenesysChat> chats = new BindingList<GenesysChat>();
        public BindingList<GenesysChat> Chats { get { return chats; } }

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

        protected override void OnParentUpdated(object message, UpdateResult result)
        {
            if (message == null && User.UserResource != null)
            {
                // TODO: improve to update differences only
                if (User.UserResource.calls != null)
                    foreach (var callResource in User.UserResource.calls)
                        calls.Add(new GenesysCall(this, callResource));
                
                RaisePropertyChanged(result.Notifications, "ActiveCall");

                chats.Clear();
                if (User.UserResource.chats != null)
                    foreach (var chatResource in User.UserResource.chats)
                        chats.Add(new GenesysChat(this, chatResource));

                RaisePropertyChanged(result.Notifications, "ActiveChat");
                RaisePropertyChanged(result.Notifications, "ActiveChatMessages");
            }
            else
            {
                var genesysEvent = message as GenesysEvent;
                if (genesysEvent != null && genesysEvent.MessageType == "CallStateChangeMessage")
                {
                    var callResource = genesysEvent.GetResourceAsType<CallResource>("call");
                    var call = Calls.FirstOrDefault(c => c.Id == callResource.id);
                    bool isNewCall = call == null;
                    if (isNewCall)
                    {
                        call = new GenesysCall(this, callResource);
                        if (!call.Finished)
                        {
                            Calls.Add(call);
                        }
                    }
                    else
                    {
                        call.HandleEvent(result.Notifications, genesysEvent.NotificationType, callResource);

                        if (call.Finished)
                        {
                            Calls.Remove(call);
                        }
                    }

                    // Quick temporary solution: always notify changes on ActiveCall
                    RaisePropertyChanged(result.Notifications, "ActiveCall");
                }

                if (genesysEvent != null && genesysEvent.MessageType == "ChatStateChangeMessage")
                {
                    var chatResource = genesysEvent.GetResourceAsType<ChatResource>("chat");
                    var chat = Chats.FirstOrDefault(c => c.Id == chatResource.id);
                    bool isNewChat = chat == null;
                    if (isNewChat)
                    {
                        chat = new GenesysChat(this, chatResource);
                        if (!chat.Finished)
                        {
                            Chats.Add(chat);
                        }
                    }
                    else
                    {
                        chat.HandleEvent(result.Notifications, genesysEvent.NotificationType, chatResource);

                        if (chat.Finished)
                        {
                            Chats.Remove(chat);
                        }
                    }

                    // Quick temporary solution: always notify changes on ActiveChat
                    RaisePropertyChanged(result.Notifications, "ActiveChat");
                }

                if (genesysEvent != null && genesysEvent.MessageType == "MessageLogUpdated")
                {
                    var chat = Chats.FirstOrDefault(c => "/chats/" + c.Id == (string)genesysEvent.Data["chatPath"]);
                    chat.HandleMessageLogUpdatedEvent(result.Notifications, genesysEvent);

                    // Quick temporary solution: always notify changes on ActiveChat
                    RaisePropertyChanged(result.Notifications, "ActiveChatMessages");
                }
            }
        }
       
        public GenesysCall ActiveCall { get { return calls.FirstOrDefault(); } }

        public GenesysChat ActiveChat { get { return chats.FirstOrDefault(); } }

        public BindingList<GenesysChatMessage> ActiveChatMessages { get { return ActiveChat ==  null ? null : ActiveChat.Messages; } }
    }
}
