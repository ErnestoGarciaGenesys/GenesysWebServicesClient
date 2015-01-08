using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Resources
{
    public class UserState
    {
        [ReadOnly(true)]
        public string Id { get; set; }

        [ReadOnly(true)]
        public string State { get; set; }

        [ReadOnly(true)]
        public string DisplayName { get; set; }
    }
}
