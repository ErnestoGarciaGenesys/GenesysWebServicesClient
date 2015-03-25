using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Components
{
    public class NotifyPropertyChangedSupport : INotifyPropertyChanged
    {
        #region Attributes

        public IDictionary<string, object> ResourceData { get; set; }

        protected object GetAttributeByName(string attributeName)
        {
            if (ResourceData == null)
                return null;

            object valueObj;
            ResourceData.TryGetValue(attributeName, out valueObj);
            return valueObj;
        }

        protected object GetAttribute([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return GetAttributeByName(FirstToLower(propertyName));
        }

        protected void UpdateAttributes(IDelayedEvents doLast, IDictionary<string, object> newResourceData)
        {
            var oldResource = ResourceData;

            foreach (var attrib in newResourceData)
            {
                if (AttributeChanged(attrib.Key, attrib.Value, oldResource))
                    RaisePropertyChanged(doLast, FirstToUpper(attrib.Key));
            }

            RaiseUpdated(doLast);
            ResourceData = newResourceData;
        }

        static bool AttributeChanged(string attribName, object newVal, IDictionary<string, object> oldRes)
        {
            if (oldRes == null)
                return true;

            object oldVal;
            if (oldRes.TryGetValue(attribName, out oldVal))
            {
                if (newVal is string && oldVal is string)
                {
                    // compare for strings
                    return (string)newVal != (string)oldVal;
                }
                else
                {
                    // assume changed for any other types not considered for comparison
                    return true;
                }
            }
            else
            {
                // there was no previous value
                return true;
            }
        }

        public event EventHandler Updated;

        protected void RaiseUpdated(IDelayedEvents postEvents)
        {
            postEvents.Add(() =>
            {
                if (Updated != null)
                    Updated(this, EventArgs.Empty);
            });
        }

        static string FirstToLower(string s)
        {
            return Char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        static string FirstToUpper(string s)
        {
            return Char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        #endregion Attributes

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public interface IDelayedEvents
        {
            void Add(Action a);
        }

        protected void RaisePropertyChanged(IDelayedEvents ev, string propertyName)
        {
            ev.Add(() =>
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            });
        }
        protected void SetPropertyValue(string propertyName, object value)
        {
            GetType().GetProperty(propertyName).SetValue(this, value);
        }

        protected void ChangeAndNotifyProperty(IDelayedEvents ev, string propertyName, object value)
        {
            // TODO: check if value has really changed? Does not work for collection objects for example.
            SetPropertyValue(propertyName, value);
            RaisePropertyChanged(ev, propertyName);
        }

        #endregion INotifyPropertyChanged

        #region Utility for Windows Forms Data Binding

        BindingSource bindingSource;

        /// <summary>
        /// This method will bind a Windows Forms control property to a property path of this component.
        /// <para>
        /// This binding behaves correctly for property paths with more than one property (like "UserState.DisplayName"), and
        /// in the case that any of the properties in the path have a <c>null</c>value.
        /// </para>
        /// </summary>
        /// <param name="propertyPath">Property path of this component to bind to.</param>
        /// <param name="control">Control to bind.</param>
        /// <param name="controlPropertyName">Name of the control property to bind.</param>
        /// <returns>The newly created binding.</returns>
        public Binding BindControl(string propertyPath, Control control, string controlPropertyName)
        {
            // Issues with Windows Forms Data Binding:
            //
            // - When specifying a DataMember path (like "ParentProp.ChildProp"):
            //     - All properties in the path must be defined and typed.
            //       (Either with native properties or with custom properties defined by implementing ICustomTypeDescriptor).
            //     - A BindingSource must be used for wrapping the DataSource.
            //       A plain Binding will not work, as it will throw an ArgumentNullException when trying to register an event with propDesc.AddValueChanged().
            //
            // - Windows Forms does not support binding to dynamic objects introduced in .NET 4 (IDynamicMetaObjectProvider, ExpandoObject...)

            bool isPath = propertyPath.Contains('.');
            object dataSource;

            if (isPath)
            {
                if (bindingSource == null)
                {
                    bindingSource = new BindingSource();
                    bindingSource.DataSource = this;
                    // bindingSource.DataMember is not set, as it may be null, which would result in an invalid data source.
                }

                dataSource = bindingSource;
            }
            else
            {
                dataSource = this;
            }

            // DataMember is set here.
            var binding = control.DataBindings.Add(controlPropertyName, dataSource, propertyPath);
            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            if (isPath)
                // The BindingSource does not do an initial update if one of the DataMember properties in its path is null,
                // that is why an initial update is requested explicitely here.
                binding.ReadValue();

            return binding;
        }

        #endregion Utility for Windows Forms Data Binding
    }
}
