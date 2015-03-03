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
