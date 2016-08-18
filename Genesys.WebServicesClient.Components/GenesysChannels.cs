using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysChannels : DynamicResource<ChannelResource>
    {
        internal bool HandleEvent(GenesysEvent genesysEvent)
        {
            if (genesysEvent.MessageType == "ChannelStateChangeMessageV2")
            {
                var channelResources = new Dictionary<string, ChannelResource>();
                var channels = genesysEvent.GetResourceAsType<IReadOnlyList<ChannelResource>>("channels");
                foreach (var channel in channels)
                {
                    channelResources.Add(channel.channel, channel);
                }
                return Update(channelResources);
            }
            else
            {
                return false;
            }
        }
    }
}
