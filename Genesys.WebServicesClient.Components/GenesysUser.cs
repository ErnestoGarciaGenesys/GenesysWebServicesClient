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
            get { return (GenesysConnection)ParentComponent; }
            set { this.ParentComponent = value; }
        }

        IEventSubscription eventSubscription;

        protected override void ActivateImpl()
        {
            eventSubscription = Connection.EventReceiver.SubscribeAll(HandleEvent);
            RefreshAgentAsync();
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
            var user = e.GetResourceAsTypeOrNull<IDictionary<string, object>>("user");

            if (GenesysEventReceivedInternal != null)
                GenesysEventReceivedInternal(e);
        }

        async void RefreshAgentAsync()
        {
            // doc: http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together
            var response = await Connection.Client.CreateRequest("GET", "/api/v2/me?subresources=*").SendAsync<UserResourceResponse>();

            var userResource = (IDictionary<string, object>)response.AsDictionary["user"];
            var untypedSettings = (IDictionary<string, object>)userResource["settings"];
            Settings = untypedSettings.ToDictionary(kvp => kvp.Key, kvp => (IDictionary<string, object>)kvp.Value);

            RaiseResourceUpdated();
        }

        // TODO: include event or resource info
        void RaiseResourceUpdated()
        {
            if (ResourceUpdated != null)
                ResourceUpdated(this, EventArgs.Empty);
        }
        
        public IDictionary<string, IDictionary<string, object>> Settings { get; private set; }
    }
}
