using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient
{
    public class OpenFailedException : Exception
    {
        public OpenFailedException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public OpenFailedException(string message) :
            this(message, null)
        {
        }
    }
}
