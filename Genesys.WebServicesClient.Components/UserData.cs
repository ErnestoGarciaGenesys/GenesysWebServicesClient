using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class UserData : DynamicResource<object>
    {
        public UserData(InteractionResource interactionResource)
        {
            Update(interactionResource.userData);
        }

        internal bool UpdateOnEvent(string notificationType, InteractionResource interactionResource)
        {
            return Update(interactionResource.userData);
        }
    }
}
