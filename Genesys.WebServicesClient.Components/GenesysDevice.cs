using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysDevice : NotifyPropertyChangedComponent
    {
        GenesysUser user;

        [Category("Activation")]
        public GenesysUser User
        {
            get { return user; }
            set
            {
                if (user != null && user != value)
                    throw new InvalidOperationException("User can only be set once");

                user = value;
                value.ResourceUpdatedInternal += User_ResourceUpdatedInternal;
                value.GenesysEventReceivedInternal += User_GenesysEventReceivedInternal;
            }
        }

        int deviceIndex = 0;

        [Category("Activation")]
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

        protected override void Dispose(bool disposing)
        {
            id = null;

            if (disposing)
                if (User != null)
                {
                    User.ResourceUpdatedInternal -= User_ResourceUpdatedInternal;
                    User.GenesysEventReceivedInternal -= User_GenesysEventReceivedInternal;
                }

            base.Dispose(disposing);
        }

        void User_GenesysEventReceivedInternal(GenesysEvent e)
        {
            if (e.MessageType == "DeviceStateChangeMessage")
                RefreshDevice(e.GetResourceAsType<IReadOnlyList<DeviceResource>>("devices"));
        }

        void User_ResourceUpdatedInternal(UserResource user)
        {
            RefreshDevice(user.devices);
        }

        void RefreshDevice(IReadOnlyList<DeviceResource> devices)
        {
            var device = ObtainDevice(devices);
            if (device == null)
                return;

            ChangeAndNotifyProperty("UserState", device.userState);
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
        [Bindable(BindableSupport.Yes),
        ReadOnly(true),
        CategoryAttribute("Read-only")]
        public UserState UserState
        {
            get;
            private set;
        }
    }
}
