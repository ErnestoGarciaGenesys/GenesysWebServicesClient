using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.ObservableResource
{

    class ResourceDeclarationSample
    {
        readonly Resource userResource = new Resource(new Resource.ResourceDescription()
        {
            SimpleKeys = new string[] { "id", "userName" },
            ArrayResources = new Resource.ArrayResourceDescription[]
                    {
                        new Resource.ArrayResourceDescription()
                        {
                            ArrayResourceKey = "devices",
                            Index = 0,
                            ResourceIdKey = "id",
                            ResourceDescription = new Resource.ResourceDescription()
                            {
                                SimpleKeys = new string[] { "id" },
                                ArrayResources = new Resource.ArrayResourceDescription[] {},
                            },
                        },
                    },
        });
    }

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
            if (attributes.Length == 0)
                return GetProperties();
            else
                return PropertyDescriptorCollection.Empty;
        }
    }

    public class Resource : AbstractCustomTypeDescriptor, INotifyPropertyChanged
    {
        abstract class AbstractPropertyDescriptor : PropertyDescriptor
        {
            public AbstractPropertyDescriptor(string name)
                : base(name, new Attribute[] {})
            {
            }

            public override bool IsReadOnly
            {
                get
                {
                    throw new NotImplementedException();
                    //return true;
                }
            }

            public override Type ComponentType
            {
                get
                {
                    throw new NotImplementedException();
                    //return resource.GetType();
                }
            }

            public override bool CanResetValue(object component)
            {
                throw new NotImplementedException();
                //return false;
            }

            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override bool ShouldSerializeValue(object component)
            {
                throw new NotImplementedException();
                //return false;
            }
        }

        class SimplePropertyDescriptor : AbstractPropertyDescriptor
        {
            readonly Resource resource;

            public SimplePropertyDescriptor(Resource resource, string name)
                : base(name)
            {
                this.resource = resource;
            }

            public override object GetValue(object component)
            {
                return GetValueOrNull();
            }

            object GetValueOrNull()
            {
                object result;
                resource.data.TryGetValue(Name, out result);
                return result;
            }

            public override void SetValue(object component, object value)
            {
                //this.value = value;
                //OnValueChanged(component, EventArgs.Empty);
            }

            public override Type PropertyType
            {
                get
                {
                    return typeof(object);
                    //object value = GetValueOrNull();
                    //return value == null ?
                    //    typeof(object) :
                    //    value.GetType();
                }
            }
        }

        class ArrayResourcePropertyDescriptor : AbstractPropertyDescriptor
        {
            readonly Resource parentResource;
            readonly Resource resource;
            readonly string key;
            readonly int index;

            public ArrayResourcePropertyDescriptor(Resource parentResource, string key, int index, Resource resource)
                : base(key + '_' + index)
            {
                this.parentResource = parentResource;
                this.key = key;
                this.index = index;
                this.resource = resource;
            }

            public string Key { get { return key; } }
            public int Index { get { return index; } }
            public Resource Resource { get { return resource; } }
            
            public override object GetValue(object component)
            {
                Resource result;
                parentResource.arrayResources.TryGetValue(Name, out result);
                return result;
            }

            public override void SetValue(object component, object value)
            {
                //this.value = value;
                //OnValueChanged(component, EventArgs.Empty);
            }

            public override Type PropertyType
            {
                get
                {
                    return typeof(Resource);
                    //object value = GetValueOrNull();
                    //return value == null ?
                    //    typeof(object) :
                    //    value.GetType();
                }
            }
        }
    
        IDictionary<string, object> data = new Dictionary<string, object>();

        IEnumerable<ArrayResourcePropertyDescriptor> arrayResourceProperties;
        IDictionary<string, Resource> arrayResources = new Dictionary<string, Resource>();

        public class ResourceDescription
        {
            public string[] SimpleKeys { get; set; }
            public ArrayResourceDescription[] ArrayResources { get; set; }
        }

        public class ArrayResourceDescription
        {
            public string ArrayResourceKey { get; set; }
            public int Index { get; set; }
            public ResourceDescription ResourceDescription { get; set; }
            public string ResourceIdKey { get; set; }
        }

        public Resource(ResourceDescription resourceDescription)
        {
            IEnumerable<PropertyDescriptor> simpleProperties = resourceDescription.SimpleKeys.Select(
                p => new SimplePropertyDescriptor(this, p));
            arrayResourceProperties = resourceDescription.ArrayResources.Select(
                d => new ArrayResourcePropertyDescriptor(this, d.ArrayResourceKey, d.Index, new Resource(d.ResourceDescription)));
            SetPropertyDescriptors(simpleProperties.Union(arrayResourceProperties));
        }

        public void Update(IDictionary<string, object> data)
        {
            this.data = data;

            foreach (var p in arrayResourceProperties)
            {
                object arrayObj;
                data.TryGetValue(p.Key, out arrayObj);
                var array = arrayObj as object[];
                if (array != null && array.Length > p.Index)
                {
                    var value = array[p.Index] as IDictionary<string, object>;
                    if (value != null)
                    {
                        arrayResources.Add(p.Name, p.Resource);
                        p.Resource.Update(value);
                    }
                }
            }

            if (PropertyChanged != null)
                foreach (var p in PropertyDescriptors)
                    PropertyChanged(this, new PropertyChangedEventArgs(((PropertyDescriptor)p).Name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get {
            object result;
            data.TryGetValue("id", out result);
            return result as string;
        }}
    }
}
