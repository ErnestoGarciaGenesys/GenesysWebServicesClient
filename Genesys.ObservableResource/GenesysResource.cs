using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysResource : INotifyPropertyChanged, IComponent
    {
        IDictionary<string, object> attributeValues = new Dictionary<string, object>();

        // Example of a constructor that obtains the IContainer
        //public GenesysResource(IContainer container)
        //{
        //    Console.WriteLine("IContainer is " + container);
        //}

        protected object GetAttributeValue(string attributeName)
        {
            object value;
            return attributeValues.TryGetValue(attributeName, out value);
        }
        
        public event EventHandler Disposed;

        public ISite Site { get; set; }

        public void Dispose() { }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
