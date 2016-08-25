using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysChannelManager : GenesysComponent
    {
        readonly GenesysChannels channels = new GenesysChannels();
        public GenesysChannels Channels { get { return channels; } }
        
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
            var genesysEvent = message as GenesysEvent;
            if (genesysEvent != null)
            {
                var channelsChanged = channels.HandleEvent(genesysEvent);
                if (channelsChanged)
                    RaisePropertyChanged(result.Notifications, "Channels");
            }
        }

    }
}
