using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public abstract class AbstractCustomTypeDescriptor : CustomTypeDescriptor
    {
        PropertyDescriptorCollection propertyDescriptors;
        protected PropertyDescriptorCollection PropertyDescriptors { get { return propertyDescriptors; } }

        protected void SetPropertyDescriptors(IEnumerable<PropertyDescriptor> propertyDescriptors)
        {
            this.propertyDescriptors = new PropertyDescriptorCollection(propertyDescriptors.ToArray());
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return propertyDescriptors;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            //if (attributes.Length == 0)
                return GetProperties();
            //else
            //    return PropertyDescriptorCollection.Empty;
        }
    }

    abstract class AbstractPropertyDescriptor : PropertyDescriptor
    {
        public AbstractPropertyDescriptor(string name)
            : base(name, new Attribute[] { })
        {
        }

        public override Type ComponentType
        {
            get
            {
                return this.GetType();
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }

    class ValuePropertyDescriptor : AbstractPropertyDescriptor
    {
        object value;

        public ValuePropertyDescriptor(string name, object value)
            : base(name)
        {
            this.value = value;
        }

        public object Value {
            get { return value; }
            set { this.value = value; }
        }

        public override object GetValue(object component)
        {
            return value;
        }

        public override Type PropertyType
        {
            get { return this.value.GetType(); }
        }

        public override void SetValue(object component, object value)
        {
 	        throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A resource with a dynamic change of attributes. Therefore, this object properties
    /// (returned by GetProperties) change at runtime.
    /// </summary>
    public class DynamicResource : AbstractCustomTypeDescriptor, INotifyPropertyChanged
    {
        IDictionary<string, ValuePropertyDescriptor> attributeDescriptors = new Dictionary<string, ValuePropertyDescriptor>();

        protected bool Update(IDictionary<string, object> newAttributeValues)
        {
            if (newAttributeValues == null)
                newAttributeValues = new Dictionary<string, object>();

            bool attributesChanged = attributeDescriptors.Count != newAttributeValues.Count;

            foreach (var attribute in newAttributeValues)
            {
                ValuePropertyDescriptor descriptor;
                attributeDescriptors.TryGetValue(attribute.Key, out descriptor);
                if (descriptor == null)
                {
                    attributesChanged = true;
                    descriptor = new ValuePropertyDescriptor(attribute.Key, attribute.Value);
                    attributeDescriptors.Add(attribute.Key, descriptor);
                    RaisePropertyChanged(attribute.Key);
                }
                else if (attribute.Value is int || attribute.Value is string)
                {
                    if (attribute.Value != descriptor.Value)
                    {
                        descriptor.Value = attribute.Value;
                        RaisePropertyChanged(attribute.Key);
                    }
                }
                else
                {
                    descriptor.Value = attribute.Value;
                }
            }

            if (attributesChanged)
                SetPropertyDescriptors(attributeDescriptors.Values);

            return attributesChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CallUserData : DynamicResource
    {
        public CallUserData(CallResource callResource)
        {
            Update(callResource.userData);
        }

        internal bool HandleEvent(string notificationType, CallResource callResource)
        {
            if (notificationType == "AttachedDataChanged")
                return Update(callResource.userData);

            return false;
        }
    }
}
