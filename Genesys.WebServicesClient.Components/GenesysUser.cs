using Genesys.WebServicesClient.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysUser : NotifyPropertyChangedComponent
    {
        [Category("Activation")]
        public GenesysConnection Connection
        {
            get { return (GenesysConnection)parentComponent; }
            set { this.parentComponent = value; }
        }

        IEventSubscription eventSubscription;

        protected override void ActivateImpl()
        {
            eventSubscription = Connection.EventReceiver.SubscribeAll(HandleEvent);
            RefreshAgent();
        }

        protected override void DeactivateImpl()
        {
            eventSubscription.Dispose();
            eventSubscription = null;
        }

        public async Task ChangeState(string value)
        {
            await Connection.Client.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        public event Action<UserResource> ResourceUpdatedInternal;
        public event Action<GenesysEvent> GenesysEventReceivedInternal;
        public event EventHandler<EventArgs> ResourceUpdated;

        void HandleEvent(object sender, GenesysEvent e)
        {
            if (GenesysEventReceivedInternal != null)
                GenesysEventReceivedInternal(e);

            RaiseResourceUpdated();
        }

        async void RefreshAgent()
        {
            // doc: http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together
            var response = await Connection.Client.CreateRequest("GET", "/api/v2/me?subresources=*").SendAsync<UserResourceResponse>();

            RaiseResourceUpdated();
        }

        // TODO: include event or resource info
        void RaiseResourceUpdated()
        {
            if (ResourceUpdated != null)
                ResourceUpdated(this, EventArgs.Empty);
        }
    }
}
