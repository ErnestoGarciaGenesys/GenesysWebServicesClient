using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient
{
    class InvalidGenesysResponseException : Exception
    {
        public InvalidGenesysResponseException(string message)
            : base(message) { }

        public InvalidGenesysResponseException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
