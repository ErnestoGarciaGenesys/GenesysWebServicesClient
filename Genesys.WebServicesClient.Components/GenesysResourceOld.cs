using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysResourceOld : CustomTypeDescriptor, INotifyPropertyChanged //, ICurrencyManagerProvider
    {
        readonly IDictionary<string, ResourcePropertyDescriptor> properties = new Dictionary<string, ResourcePropertyDescriptor>();
        readonly ExtendablePropertyDescriptorCollection propertyCollection;

        readonly internal ICollection<string> propertyNamesInResourceManager = new List<string>();

        public GenesysResourceOld Self { get { return this; } }

        public GenesysResourceOld Null { get { return null; } }

        public String Test { get { return "testing"; } }

        public GenesysResourceOld()
        {
            this.propertyCollection = new ExtendablePropertyDescriptorCollection(this);
            TypeDescriptor.AddProvider(new CustomTypeDescriptionProvider(this), typeof(GenesysResourceOld));
            var testprops = TypeDescriptor.GetProperties(this);
            var testprops2 = TypeDescriptor.GetProperties(typeof(GenesysResourceOld));
            var testprops3 = TypeDescriptor.GetProperties(typeof(GenesysResourceOld), new Attribute[] { new BrowsableAttribute(true) });
        }

        public bool Update(IDictionary<string, object> newData)
        {
            // needed?
            //if (newData == null)
            //    newData = new Dictionary<string, object>();

            bool propertyListChanged = newData.Count != properties.Count;

            foreach (var d in newData)
            {
                //Update(FirstLetterToUpper(d.Key), d.Value, ref propertyListChanged);
                Update(d.Key, d.Value, ref propertyListChanged);
            }

            return propertyListChanged;
        }

        //string FirstLetterToUpper(string str)
        //{
        //    return char.ToUpperInvariant(str[0]) + str.Substring(1);
        //}

        void Update(string propertyName, object newValue, ref bool propertyListChanged)
        {
            ResourcePropertyDescriptor existingProperty;
            properties.TryGetValue(propertyName, out existingProperty);
            if (existingProperty == null)
            {
                var newProperty = new ResourcePropertyDescriptor(propertyName) { Value = newValue };
                properties.Add(propertyName, newProperty);
                propertyListChanged = true;
            }
            else
            {
                bool changeValue = newValue is int || newValue is string ?
                    newValue != existingProperty.Value :
                    true;

                if (changeValue)
                {
                    existingProperty.Value = newValue;
                    RaisePropertyChanged(propertyName);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        class CustomTypeDescriptionProvider : TypeDescriptionProvider
        {
            readonly GenesysResourceOld resource;

            public CustomTypeDescriptionProvider(GenesysResourceOld resource)
            {
                this.resource = resource;
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                return resource;
            }

            public override System.Collections.IDictionary GetCache(object instance)
            {
                var result = base.GetCache(instance);
                return result;
            }
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return propertyCollection;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return base.GetProperties();
        }
        
        class ExtendablePropertyDescriptorCollection : PropertyDescriptorCollection
        {
            readonly GenesysResourceOld resource;

            public ExtendablePropertyDescriptorCollection(GenesysResourceOld resource)
                : base(resource.properties.Values.ToArray<PropertyDescriptor>(), false)
            {
                this.resource = resource;
            }

            public override PropertyDescriptor Find(string name, bool ignoreCase)
            {
                var prop = base.Find(name, ignoreCase);
                if (prop == null)
                {
                    var newProp = new ResourcePropertyDescriptor(name);
                    resource.properties.Add(name, newProp);
                    Add(newProp);
                    TypeDescriptor.Refresh(resource);
                    return newProp;
                }
                else
                {
                    return prop;
                }
            }
        }



        //class ThisCustomTypeDescriptor : CustomTypeDescriptor
        //{
        //    GenesysResourceOld resource;

        //    public ThisCustomTypeDescriptor(GenesysResourceOld resource)
        //    {
        //        this.resource = resource;
        //    }

        //    public override PropertyDescriptorCollection GetProperties()
        //    {
        //        return resource.propertyCollection;
        //    }
        //}

        class ResourcePropertyDescriptor : PropertyDescriptor
        {
            internal object Value { get; set; }

            public ResourcePropertyDescriptor(string propertyName)
                : base(propertyName, new Attribute[0]) { }

            public override object GetValue(object component) { return Value; }
            
            public override Type PropertyType
            {
                get
                {
                    if (Name == "Self")
                        return typeof(GenesysResourceOld);

                    return Value == null ?
                        typeof(object) :
                        Value.GetType(); 
                } 
            }

            public override bool IsReadOnly { get { return true; } }
            public override void SetValue(object component, object value) { }
            public override Type ComponentType { get { return typeof(GenesysResourceManagerOld); } }
            public override bool CanResetValue(object component) { return false; }
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) { return false; }
        }

        //CurrencyManager ICurrencyManagerProvider.CurrencyManager
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //CurrencyManager ICurrencyManagerProvider.GetRelatedCurrencyManager(string dataMember)
        //{
        //    if (string.IsNullOrEmpty(dataMember))
        //        return null;

        //    if (TypeDescriptor.GetProperties(this).Find(dataMember, true) == null)
        //    {
        //        properties.Add(dataMember, new ResourcePropertyDescriptor(dataMember));
        //        TypeDescriptor.Refresh(this);
        //    }

        //    return null;
        //}

        //class MyCurrencyManager : CurrencyManager
        //{
        //    public override PropertyDescriptorCollection GetItemProperties()
        //    {
        //        return base.GetItemProperties();
        //    }
        //}
    }
}
