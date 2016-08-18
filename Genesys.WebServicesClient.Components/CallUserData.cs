using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class CallUserData : DynamicResource<object>
    {
        public CallUserData(CallResource callResource)
        {
            Update(callResource.userData);
        }

        internal bool HandleEvent(string notificationType, CallResource callResource)
        {
            if (notificationType == "AttachedDataChanged")
            {
                return Update(callResource.userData);
            }
            else
            {
                return false;
            }
        }
    }
}
