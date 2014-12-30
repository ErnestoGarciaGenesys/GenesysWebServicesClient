using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public abstract class NotifyPropertyChangedComponent : AutoInitComponent, INotifyPropertyChanged
    {
        IDictionary<string, object> attributeValues = new Dictionary<string, object>();

        protected object GetAttributeValue(string attributeName)
        {
            object value;
            return attributeValues.TryGetValue(attributeName, out value);
        }

        protected void ChangeAndNotifyProperty(string propertyName, object value)
        {
            // TODO: check if value has really changed
            GetType().GetProperty(propertyName).SetValue(this, value);
            RaisePropertyChanged(propertyName);
        }

        // An alternative for notifying from within the setter method. No need to use property names strings this way.
        protected void RaiseThisPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            RaisePropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
