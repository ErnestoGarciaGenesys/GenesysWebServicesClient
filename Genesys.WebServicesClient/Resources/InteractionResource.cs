using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Resources
{
    public abstract class InteractionResource
    {
        public string id;
        public string state;
        public IList<string> capabilities;
        public IDictionary<string, object> userData;
        public IList<object> participants;
    }
}
