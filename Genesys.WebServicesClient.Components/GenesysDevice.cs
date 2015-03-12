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

        string id;

        protected override void OnParentUpdated(InternalUpdatedEventArgs e)
        {
            if (e.GenesysEvent == null)
            {
                RefreshDevice(e.DelayedEvents, User.UserResource.devices);
            }
            else
            {
                if (e.GenesysEvent.MessageType == "DeviceStateChangeMessage")
                    RefreshDevice(e.DelayedEvents, e.GenesysEvent.GetResourceAsType<IReadOnlyList<DeviceResource>>("devices"));
            }
        }

        void RefreshDevice(IDelayedEvents postEvents, IReadOnlyList<DeviceResource> devices)
        {
            var device = ObtainDevice(devices);
            if (device == null)
                return;

            ChangeAndNotifyProperty(postEvents, "UserState", device.userState);
        }

        DeviceResource ObtainDevice(IReadOnlyList<DeviceResource> devices)
        {
            DeviceResource device = null;
            if (id == null)
            {
                if (devices.Count() > 0)
                {
                    device = devices[deviceIndex];
                    id = device.id;
                }
            }
            else
            {
                device = devices.FirstOrDefault(d => id == d.id);
            }

            return device;
        }

        // [Browsable(false)], needs to be Browsable for enabling data binding to its properties.
        [Bindable(BindableSupport.Yes), ReadOnly(true)]
        public UserState UserState
        {
            get;
            private set;
        }
    }
}
