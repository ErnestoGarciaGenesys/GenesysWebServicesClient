using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysUser2 : GenesysResource
    {
        // Example of ctor with passed container
        //public GenesysUser2(IContainer container)
        //    : base(container)
        //{

        //}

        public string FirstName
        {
            get { return (string)GetAttributeValue("firstName"); }
        }
    }
}
