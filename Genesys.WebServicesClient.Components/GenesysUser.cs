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
        GenesysConnection connection;
        internal GenesysEventReceiver EventReceiver { get; private set; }
        public string State { get; private set; }

        internal GenesysCallManager CallManager;

        protected override void ActivateImpl()
        {
            EventReceiver = Connection.Client.CreateEventReceiver();
            EventReceiver.Open();
            EventReceiver.SubscribeAll(HandleEvent);
            RefreshAgent();
        }

        protected override void DeactivateImpl()
        {
            if (EventReceiver != null)
                EventReceiver.Dispose();
        }

        /// <summary>
        /// Connection is mandatory.
        /// </summary>
        public GenesysConnection Connection
        {
            get { return connection; }
            set
            {
                this.connection = value;
                this.parentComponent = value;
            }
        }

        public async Task ChangeState(string value)
        {
            await Connection.Client.CreateRequest("POST", "/api/v2/me", new { operationName = value }).SendAsync();
        }

        public Task MakeReady()
        {
            return ChangeState("Ready");
        }

        public Task MakeNotReady()
        {
            return ChangeState("NotReady");
        }

        public event Action<IDictionary<string, object>> UserResourceUpdated;
        public event Action<GenesysEvent> EventHandlingFinished;

        void HandleEvent(object sender, GenesysEvent e)
        {
            if (e.MessageType == "DeviceStateChangeMessage")
                RefreshAgentState(e.GetResourceAsType<IEnumerable<DeviceResource>>("devices"));

            if (CallManager != null)
                CallManager.HandleEvent(e);

            if (EventHandlingFinished != null)
                EventHandlingFinished(e);
        }

        async void RefreshAgent()
        {
            // doc: http://docs.genesys.com/Documentation/HTCC/8.5.2/API/RecoveringExistingState#Reading_device_state_and_active_calls_together
            var response = await Connection.Client.CreateRequest("GET", "/api/v2/me?subresources=*").SendAsync<UserResourceResponse>();

            RefreshAgentState(response.AsType.user.devices);
            if (UserResourceUpdated != null)
                UserResourceUpdated(response.AsDictionary);
        }
        
        void RefreshAgentState(IEnumerable<DeviceResource> devices)
        {
            ChangeAndNotifyProperty("State", devices.First().userState.state);
        }
    }
}
