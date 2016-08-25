using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Resources
{
    public class MessageResource
    {
        public int index;
        public string type;
        public ParticipantResource from;
        public string text;
        public string visibility;
        public string timestamp;
        public long timestampSeconds;
    }
}
