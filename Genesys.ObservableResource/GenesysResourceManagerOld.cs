using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysResourceManagerOld : CustomTypeDescriptor, INotifyPropertyChanged
    {
        readonly IDictionary<string, GenesysResourceOld> resourcesById = new Dictionary<string, GenesysResourceOld>();
        
        readonly IDictionary<string, GenesysResourceOld> resourcesByPropertyName = new Dictionary<string, GenesysResourceOld>();

        public void UpdateResource(IDictionary<string, object> data)
        {
            var resource = GetOrCreateResource(data);
            bool propertiesChanged = resource.Update(data);
            if (propertiesChanged)
                foreach (var p in resource.propertyNamesInResourceManager)
                    RaisePropertyChanged(p);
        }

        public void CreateResource(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            var resource = new GenesysResourceOld();
            resourcesByPropertyName.Add(propertyName, resource);
            resource.propertyNamesInResourceManager.Add(propertyName);
        }

        public void UpdateResource(string propertyName, IDictionary<string, object> data)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            var resource = GetOrCreateResource(data);

            GenesysResourceOld resourceByName;
            resourcesByPropertyName.TryGetValue(propertyName, out resourceByName);
            bool managerPropertyChanged = resource != resourceByName;
            if (managerPropertyChanged)
            {
                if (resourceByName != null)
                    resourceByName.propertyNamesInResourceManager.Remove(propertyName);
                resource.propertyNamesInResourceManager.Add(propertyName);
                resourcesByPropertyName[propertyName] = resource;
                RaisePropertyChanged(propertyName);
            }

            bool resourcePropertyListChanged = resource.Update(data);

            if (resourcePropertyListChanged || managerPropertyChanged)
                foreach (var p in resource.propertyNamesInResourceManager)
                    RaisePropertyChanged(p);
        }

        GenesysResourceOld GetOrCreateResource(IDictionary<string, object> data)
        {
            object idObj;
            data.TryGetValue("id", out idObj);
            string id = idObj as string;
            if (id == null)
                throw new ArgumentException("Resource must include an 'id' field of type string.");

            GenesysResourceOld resource;
            resourcesById.TryGetValue(id, out resource);
            if (resource == null)
            {
                resource = new GenesysResourceOld();
                resourcesById.Add(id, resource);
            }

            return resource;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var propertyDescriptors = from r in resourcesByPropertyName
                                      select new ResourceManagerPropertyDescriptor(r.Key);

            return new PropertyDescriptorCollection(propertyDescriptors.ToArray());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
 	         return GetProperties();
        }

        class ResourceManagerPropertyDescriptor : PropertyDescriptor
        {
            public ResourceManagerPropertyDescriptor(string propertyName)
                : base(propertyName, new Attribute[0]) { }

            public override object GetValue(object component)
            {
                return ((GenesysResourceManagerOld)component).resourcesByPropertyName[Name];
            }

            public override Type PropertyType { get { return typeof(GenesysResourceOld); } }
            public override bool IsReadOnly { get { return true; } }
            public override void SetValue(object component, object value) {}
            public override Type ComponentType { get { return typeof(GenesysResourceManagerOld); } }
            public override bool CanResetValue(object component) { return false; }
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) { return false; }
        }
    }
}
