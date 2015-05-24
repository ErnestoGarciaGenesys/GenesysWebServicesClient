using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysDevice : GenesysComponent
    {
        #region Initialization Properties

        [Category("Initialization")]
        public GenesysUser User
        {
            get { return (GenesysUser)Parent; }
            set
            {
                if (Parent != null && Parent != value)
                    throw new InvalidOperationException("User can only be set once");

                Parent = value;
            }
        }

        int deviceIndex = 0;

        [Category("Initialization")]
        public int DeviceIndex
        {
            get { return deviceIndex; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", value, "DeviceIndex must be nonnegative");

                deviceIndex = value;
            }
        }

        #endregion Initialization Properties

        string id;

        protected override void OnParentUpdated(InternalUpdatedEventArgs e)
        {
            if (e.GenesysEvent == null)
            {
                RefreshDevice(e.PostEvents,
                    User.UserResource.devices,
                    (object[])User.ResourceData["devices"]);
            }
            else
            {
                if (e.GenesysEvent.MessageType == "DeviceStateChangeMessage")
                    RefreshDevice(e.PostEvents,
                        e.GenesysEvent.GetResourceAsType<IReadOnlyList<DeviceResource>>("devices"),
                        e.GenesysEvent.GetResourceAsType<object[]>("devices"));
            }
        }

        void RefreshDevice(IPostEvents doLast, IReadOnlyList<DeviceResource> devices, object[] devicesData)
        {
            DeviceResource device = null;
            IDictionary<string, object> newDeviceData = null;
            if (id == null)
            {
                if (devices.Count() > 0)
                {
                    device = devices[deviceIndex];
                    id = device.id;
                    newDeviceData = (IDictionary<string, object>)devicesData[deviceIndex];
                }
            }
            else
            {
                var i = devices.ToList().FindIndex(d => id == d.id);
                if (i >= 0)
                {
                    device = devices[i];
                    newDeviceData = (IDictionary<string, object>)devicesData[i];
                }
            }

            if (device == null)
                return;

            UpdateAttributes(doLast, newDeviceData);

            ChangeAndNotifyProperty(doLast, "UserState", device.userState);
        }

        // [Browsable(false)], needs to be Browsable for enabling data binding to its properties.
        [Bindable(BindableSupport.Yes), ReadOnly(true)]
        public UserState UserState
        {
            get;
            private set;
        }

        #region Attributes

        //"id": "bf78972f-a0f3-44da-9ca6-0a9a7ec5dbea",
        //"deviceState": "Active",
        //"userState": {
        //    "state": "LoggedIn"
        //},
        //"phoneNumber": "1000",
        //"e164Number": "1000",
        //"telephonyNetwork": "Private",
        //"doNotDisturb": "Off",
        //"voiceEnvironmentUri": "http://192.168.154.128/api/v2/voice-environments/d0973689-7156-4e84-bfa5-d78f6629d82c",
        //"capabilities": ["DoNotDisturbOn", "ForwardCallsOn"]

        public string Id { get { return GetAttribute() as string; } }

        public string DeviceState { get { return GetAttribute() as string; } }

        public string PhoneNumber { get { return GetAttribute() as string; } }

        public string E164Number { get { return GetAttribute() as string; } }

        public string TelephonyNetwork { get { return GetAttribute() as string; } }

        public string DoNotDisturb { get { return GetAttribute() as string; } }

        #endregion Attributes
    }
}
