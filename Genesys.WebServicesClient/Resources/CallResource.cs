using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Resources
{
    public class CallResource
    {
        public string id;
        public string state;
        public IList<string> capabilities;
        public IDictionary<string, object> userData;
        public IList<object> participants;
    }
}
