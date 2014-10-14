using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Genesys.WebServicesClient
{
    class GenesysMethodException : Exception
    {
        public readonly HttpStatusCode HttpStatusCode;
        public readonly int StatusCode;
        public readonly string StatusMessage;

        public GenesysMethodException(HttpStatusCode httpStatusCode, int statusCode, string statusMessage)
            : base(string.Format("{0} (status code {1}, HTTP {2}: {3})", statusMessage, statusCode, (int)httpStatusCode, httpStatusCode))
        {
            this.HttpStatusCode = httpStatusCode;
            this.StatusCode = statusCode;
            this.StatusMessage = statusMessage;
        }
    }
}
