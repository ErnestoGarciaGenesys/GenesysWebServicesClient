using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient
{
    public interface IGenesysResponse<T>
        where T : GenesysTypedResponseBase
    {
        string AsString { get; }
        IDictionary<string, object> AsDictionary { get; }
        T AsType { get; }
    }
}
