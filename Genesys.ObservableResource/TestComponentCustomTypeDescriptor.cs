using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class TestComponentCustomTypeDescriptor : CustomTypeDescriptor
    {
        List<PropertyDescriptor> properties = new List<PropertyDescriptor>()
        {
            new ValuedPropertyDescriptor("ExtendedProperty") { Value = "testingExtended" },
        };

        public override PropertyDescriptorCollection GetProperties()
        {
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        public void AddProperty()
        {
            properties.Add(new ValuedPropertyDescriptor("DynamicExtendedProperty") { Value = "testingDynamic" });
        }

        class ValuedPropertyDescriptor : PropertyDescriptor
        {
            internal object Value { get; set; }

            public ValuedPropertyDescriptor(string propertyName)
                : base(propertyName, new Attribute[0]) { }

            public override object GetValue(object component) { return Value; }
            public override Type PropertyType { get { return Value.GetType(); } }
            public override bool IsReadOnly { get { return true; } }
            public override void SetValue(object component, object value) { }
            public override Type ComponentType { get { return typeof(GenesysResourceManagerOld); } }
            public override bool CanResetValue(object component) { return false; }
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) { return false; }
        }
    }
}
