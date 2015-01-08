using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Components
{
    public static class GenesysWindowsFormsUtil
    {
        public static Binding AddBinding(Control control, string propertyName, object dataSource, string dataMember)
        {
            // Windows Forms issues with Data Binding:
            // - All properties in a DataMember path must be defined and typed. (It can be done with an ICustomTypeDescriptor).
            // - A BindingSource must be used for DataMembers with a path (like "ParentProp.ChildProp").
            //   A plain Binding will not work; it will throw an ArgumentNullException when trying to register an event with propDesc.AddValueChanged().
            // - Windows Forms does not support binding to dynamic objects introduced in .NET 4 (IDynamicMetaObjectProvider, ExpandoObject...)

            bool isPath = dataMember.Contains('.');
            if (isPath)
            {
                // BindingSource.DataMember is not set, as it may be null, which results in error.
                var bindingSource = new BindingSource();
                bindingSource.DataSource = dataSource;
                dataSource = bindingSource;
            }

            // DataMember is set here.
            var binding = control.DataBindings.Add(propertyName, dataSource, dataMember);
            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            if (isPath)
                // The BindingSource does not do an initial update if one of the DataMember properties in its path is null,
                // that is why an initial update is requested explicitely here.
                binding.ReadValue();

            return binding;
        }
    }
}
