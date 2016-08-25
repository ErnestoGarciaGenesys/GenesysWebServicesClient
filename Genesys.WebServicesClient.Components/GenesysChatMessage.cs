using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysChatMessage
    {
        readonly MessageResource message;
        readonly GenesysParticipant from;

        public GenesysChatMessage(MessageResource message)
        {
            this.message = message;
            this.from = new GenesysParticipant(message.from);
        }

        public int Index { get { return message.index; } }
        public string Type { get { return message.type; } }
        public string Text { get { return message.text; } }
        public string Visibility { get { return message.visibility; } }
        public string Timestamp { get { return message.timestamp; } }
        public long TimestampSeconds { get { return message.timestampSeconds; } }
        public GenesysParticipant From { get { return from; } }

    }
}
