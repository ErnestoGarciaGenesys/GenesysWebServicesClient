using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public abstract class GenesysInteraction : NotificationSupport
    {
        protected readonly GenesysInteractionManager interactionManager;
        readonly UserData userData;

        public string Id { get; private set; }
        public string State { get; protected set; }

        IList<string> capabilities;
        IReadOnlyCollection<string> readOnlyCapabilities;

        public UserData UserData { get { return userData; } }
        public IList<object> Participants { get; protected set; }

        public GenesysInteraction(GenesysInteractionManager interactionManager, InteractionResource interactionResource)
        {
            this.interactionManager = interactionManager;
            Id = interactionResource.id;
            State = interactionResource.state;
            Participants = interactionResource.participants;
            SetCapabilities(interactionResource.capabilities);
            userData = new UserData(interactionResource);
            UpdateCapableProperties(null, interactionResource.capabilities);
        }

        public abstract bool Finished { get; }

        public IReadOnlyCollection<string> Capabilities
        {
            get { return readOnlyCapabilities; }
        }

        void SetCapabilities(IList<string> value)
        {
            capabilities = value;
            readOnlyCapabilities = new ReadOnlyCollection<string>(capabilities);
        }

        internal virtual void HandleEvent(INotifications notifs, string notificationType, InteractionResource interactionResource)
        {
            if (notificationType == "StatusChange"
                || notificationType == "ParticipantsUpdated" // Can update other fields (like state), not only participants
                || notificationType == "Error")
            {
                ChangeAndNotifyProperty(notifs, "State", interactionResource.state);
                ChangeAndNotifyProperty(notifs, "Participants", interactionResource.participants);
                SetCapabilities(interactionResource.capabilities);
                UpdateCapableProperties(notifs, interactionResource.capabilities);
                RaisePropertyChanged(notifs, "Capabilities");
            }
            else if (notificationType == "PropertiesUpdated" || notificationType == "AttachedDataChanged")
            {
                bool userDataChanged = userData.UpdateOnEvent(notificationType, interactionResource);
                if (userDataChanged)
                    RaisePropertyChanged(notifs, "UserData");
            }
        }
        
        void UpdateCapableProperties(INotifications notifs, IList<string> capabilities)
        {
            foreach (var op in GetCapabilityNames())
            {
                string propertyName = op + "Capable";
                bool capable = capabilities.Contains(op);
                SetPropertyValue(propertyName, capable);

                if (notifs != null)
                    RaisePropertyChanged(notifs, propertyName);
            }
        }

        protected abstract string[] GetCapabilityNames();
    }
}
