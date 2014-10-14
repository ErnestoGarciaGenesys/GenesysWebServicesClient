using System.Collections.Generic;
using Cometd.Bayeux;

namespace Genesys.WebServicesClient
{
    public class GenesysEvent
    {
        private IMessage message;

        public GenesysEvent(IMessage message)
        {
            this.message = message;
        }

        public string Channel
        {
            get { return message.Channel; }
        }

        public IDictionary<string, object> Data
        {
            get { return message.DataAsDictionary; }
        }
    }
}
