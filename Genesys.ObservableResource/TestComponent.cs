using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class TestComponent
    {
        class CustomTypeDescriptionProvider : TypeDescriptionProvider
        {
            readonly ICustomTypeDescriptor customTypeDescriptor;

            public CustomTypeDescriptionProvider(ICustomTypeDescriptor customTypeDescriptor)
            {
                this.customTypeDescriptor = customTypeDescriptor;
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return customTypeDescriptor;
            }
        }

        readonly TestComponentCustomTypeDescriptor customTypeDescriptor = new TestComponentCustomTypeDescriptor();

        public string Property { get { return "testingProperty"; } }
        public string ExtendedProperty { get { return "overriden!!"; } }

        public TestComponent()
        {
            TypeDescriptor.AddProvider(new CustomTypeDescriptionProvider(customTypeDescriptor), this);
        }

        public void AddProperty()
        {
            TypeDescriptor.GetProvider(this);
            customTypeDescriptor.AddProperty();
            //TypeDescriptor.Refresh(this);
        }

        internal ICustomTypeDescriptor GetExtendedTypeDescriptor()
        {
            return customTypeDescriptor;
        }
    }
}
