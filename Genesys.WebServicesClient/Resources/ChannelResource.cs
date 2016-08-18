using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Resources
{
    public class ChannelResource
    {
        public string channel;
        public string Channel { get { return channel; } }

        public UserState userState;
        public UserState UserState { get { return userState; } }
    }
}
