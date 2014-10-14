using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TimeoutException"><see cref="HttpClient.Timeout"/></exception>
        public async Task<string> SendAsync()
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), uri);
            if (jsonContent != null)
                request.Content = CreateJsonContent(jsonContent);
            HttpResponseMessage response;
            try
            {
                response = await genesysClient.HttpClient.SendAsync(request);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException();
            }
            return await GetResponse(response);
        }

        StringContent CreateJsonContent(object contentObject)
        {
            string contentString = JsonSerializer.Serialize(contentObject);
            Log.TraceEvent(TraceEventType.Verbose, 20, "Request content: %s", contentString);
            return new StringContent(contentString, Encoding.UTF8, "application/json");
        }

        async Task<string> GetResponse(HttpResponseMessage httpResponse)
        {
            try
            {
                var result = await CheckResponseContent(httpResponse);
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

        async Task<string> CheckResponseContent(HttpResponseMessage httpResponse)
        {
            var responseContent = httpResponse.Content;
            if (responseContent == null)
                throw new InvalidGenesysResponseException("No content");

            string responseContentStr;
            try
            {
                responseContentStr = await responseContent.ReadAsStringAsync();
            }
            catch (InvalidOperationException e) // thrown for instance when CharSet is not a supported .NET Encoding
            {
                throw new InvalidGenesysResponseException("Unreadable content", e);
            }

            Trace.WriteLine("POST response content: " + responseContentStr);

            if (httpResponse.Content.Headers.ContentType.MediaType != "application/json")
                throw new InvalidGenesysResponseException("Content-Type of is not application/json");

            GenesysJsonResponse response;
            try
            {
                response = JsonSerializer.Deserialize<GenesysJsonResponse>(responseContentStr);
            }
            catch (ArgumentException e)
            {
                throw new InvalidGenesysResponseException("Invalid JSON", e);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidGenesysResponseException("Invalid JSON", e);
            }

            if (!response.statusCode.HasValue)
                throw new InvalidGenesysResponseException("Missing statusCode");

            if (response.statusCode.Value != 0)
                throw new GenesysMethodException(
                    httpResponse.StatusCode,
                    response.statusCode.Value,
                    response.statusMessage);

            return responseContentStr;
        }

        class GenesysJsonResponse
        {
#pragma warning disable 0649 // CS0649: Field 'field' is never assigned to, and will always have its default value
            public int? statusCode;
            public string statusMessage;
#pragma warning restore 0649
        };

    }
}
