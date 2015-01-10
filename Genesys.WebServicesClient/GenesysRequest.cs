using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Genesys.WebServicesClient
{
    public class GenesysRequest
    {
        static readonly TraceSource Log = new TraceSource(typeof(GenesysRequest).Namespace);
        static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        readonly GenesysClient genesysClient;
        readonly string httpMethod;
        readonly string uri;

        // Can be null when no JSON content.
        readonly object jsonContent;

        protected internal GenesysRequest(GenesysClient genesysClient, string httpMethod, string uri, object jsonContent)
        {
            this.genesysClient = genesysClient;
            this.httpMethod = httpMethod;
            this.uri = uri;
            this.jsonContent = jsonContent;
        }

        public Task<IGenesysResponse<GenesysTypedResponseBase>> SendAsync()
        {
            return SendAsync<GenesysTypedResponseBase>();
        }

        public Task<IGenesysResponse<T>> SendAsync<T>()
            where T : GenesysTypedResponseBase
        {
            return SendAsync<T>(CancellationToken.None);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException">On timeout, cancellation or client disposal. See <see cref="HttpClient.Timeout"/></exception>
        public async Task<IGenesysResponse<T>> SendAsync<T>(CancellationToken cancellationToken)
            where T : GenesysTypedResponseBase
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), uri);
            if (jsonContent != null)
                request.Content = CreateJsonContent(jsonContent);
            HttpResponseMessage response;

            // throws OperationCanceledException on timeout, cancellation or disposal of the HttpClient.
            response = await genesysClient.HttpClient.SendAsync(request, cancellationToken);
            
            return await GetResponse<T>(response);
        }

        StringContent CreateJsonContent(object contentObject)
        {
            string contentString = JsonSerializer.Serialize(contentObject);
            Log.TraceEvent(TraceEventType.Verbose, 20, "Request content: %s", contentString);
            return new StringContent(contentString, Encoding.UTF8, "application/json");
        }

        async Task<IGenesysResponse<T>> GetResponse<T>(HttpResponseMessage httpResponse)
            where T : GenesysTypedResponseBase
        {
            try
            {
                var result = await CheckResponseContent<T>(httpResponse);
                httpResponse.EnsureSuccessStatusCode();
                return result;
            }
            catch (InvalidGenesysResponseException)
            {
                // If the HTTP status code indicates an error, ignore an invalid content.
                httpResponse.EnsureSuccessStatusCode();
                throw;
            }
        }

        class GenesysResponse<T> : IGenesysResponse<T>
            where T : GenesysTypedResponseBase
        {
            public string AsString { get; internal set; }
            public IDictionary<string, object> AsDictionary { get; internal set; }
            public T AsType { get; internal set; }
        }

        async Task<IGenesysResponse<T>> CheckResponseContent<T>(HttpResponseMessage httpResponse)
            where T : GenesysTypedResponseBase
        {
            var responseContent = httpResponse.Content;
            if (responseContent == null)
                throw new InvalidGenesysResponseException("No content");

            string responseAsString;
            try
            {
                responseAsString = await responseContent.ReadAsStringAsync();
            }
            catch (InvalidOperationException e) // thrown for instance when CharSet is not a supported .NET Encoding
            {
                throw new InvalidGenesysResponseException("Unreadable content", e);
            }

            Trace.WriteLine("POST response content: " + responseAsString);

            if (httpResponse.Content.Headers.ContentType.MediaType != "application/json")
                throw new InvalidGenesysResponseException("Content-Type of is not application/json");

            IDictionary<string, object> responseAsDictionary;
            T responseAsType;
            try
            {
                responseAsDictionary = JsonSerializer.DeserializeObject(responseAsString) as IDictionary<string, object>;
                if (responseAsDictionary == null)
                    throw new InvalidGenesysResponseException("Invalid JSON type. Corresponding .NET type is " + responseAsDictionary.GetType().Name);
                responseAsType = JsonSerializer.ConvertToType<T>(responseAsDictionary);
            }
            catch (ArgumentException e)
            {
                throw new InvalidGenesysResponseException("Invalid JSON", e);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidGenesysResponseException("Invalid JSON", e);
            }

            if (!responseAsType.statusCode.HasValue)
                throw new InvalidGenesysResponseException("Missing statusCode");

            if (responseAsType.statusCode.Value != 0)
                throw new GenesysMethodException(
                    httpResponse.StatusCode,
                    responseAsType.statusCode.Value,
                    responseAsType.statusMessage);

            var result = new GenesysResponse<T>();

            return new GenesysResponse<T>()
            {
                AsString = responseAsString,
                AsDictionary = responseAsDictionary,
                AsType = responseAsType
            };
        }
    }
}
