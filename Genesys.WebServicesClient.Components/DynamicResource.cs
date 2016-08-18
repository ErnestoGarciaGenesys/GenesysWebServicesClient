using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class ValuePropertyDescriptor<T> : AbstractPropertyDescriptor
    {
        T value;

        public ValuePropertyDescriptor(string name, T value)
            : base(name)
        {
            this.value = value;
        }

        public T Value
        {
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
    public class DynamicResource<T> : AbstractCustomTypeDescriptor, INotifyPropertyChanged
    {
        IDictionary<string, ValuePropertyDescriptor<T>> attributeDescriptors = new Dictionary<string, ValuePropertyDescriptor<T>>();

        protected bool Update(IDictionary<string, T> newAttributeValues)
        {
            if (newAttributeValues == null)
                newAttributeValues = new Dictionary<string, T>();

            bool attributesChanged = attributeDescriptors.Count != newAttributeValues.Count;

            foreach (var attribute in newAttributeValues)
            {
                ValuePropertyDescriptor<T> descriptor;
                attributeDescriptors.TryGetValue(attribute.Key, out descriptor);
                if (descriptor == null)
                {
                    attributesChanged = true;
                    descriptor = new ValuePropertyDescriptor<T>(attribute.Key, attribute.Value);
                    attributeDescriptors.Add(attribute.Key, descriptor);
                    RaisePropertyChanged(attribute.Key);
                }
                else
                {
                    if (object.Equals(attribute.Value, descriptor.Value))
                    {
                        descriptor.Value = attribute.Value;
                    }
                    else
                    {
                        descriptor.Value = attribute.Value;
                        RaisePropertyChanged(attribute.Key);
                    }
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
}
