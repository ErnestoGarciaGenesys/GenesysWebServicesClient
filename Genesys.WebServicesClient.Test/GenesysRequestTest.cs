using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;

namespace Genesys.WebServicesClient.Test
{
    [TestClass]
    public class GenesysRequestTest
    {
        readonly MyHttpMessageHandler httpMock;
        readonly GenesysClient client;

        class MyHttpMessageHandler : HttpMessageHandler
        {
            static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

            HttpResponseMessage response;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(response);
            }

            public void setResponse(HttpResponseMessage response)
            {
                this.response = response;
            }

            public void setResponse(HttpStatusCode httpStatusCode, object content)
            {
                string contentStr = content is string ?
                    (string)content :
                    JsonSerializer.Serialize(content);

                this.response = new HttpResponseMessage(httpStatusCode);
                this.response.Content = new StringContent(contentStr);
                this.response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
        };

        public GenesysRequestTest()
        {
            httpMock = new MyHttpMessageHandler();
            var httpClient = new HttpClient(httpMock);

            client = new GenesysClient.Setup
            {
                ServerUri = "http://myserver",
                Anonymous = true,
                HttpClient = httpClient,
            }.Create();
        }

        [TestMethod]
        public void CreateRequest_with_relative_URI_succeeds()
        {
            client.CreateRequest("GET", "/relative/uri");
        }

        [TestMethod]
        public void CreateRequest_with_absolute_URI_succeeds()
        {
            client.CreateRequest("GET", "http://myserver/absolute/uri");
        }

        [TestMethod]
        public void CreateRequest_with_absolute_https_URI_succeeds()
        {
            client.CreateRequest("GET", "https://myserver/absolute/uri");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateRequest_with_wrong_URI_fails()
        {
            client.CreateRequest("GET", "wrong/url");
        }

        [TestMethod]
        public async Task SendAsync_responds_correctly()
        {
            var request = client.CreateRequest("GET", "/foo");
            httpMock.setResponse(HttpStatusCode.OK, new { statusCode = 0 });
            await request.SendAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(GenesysMethodException))]
        public async Task SendAsync_responds_with_Genesys_failure()
        {
            var request = client.CreateRequest("GET", "/foo");
            httpMock.setResponse(HttpStatusCode.InternalServerError, new { statusCode = 1, statusMessage = "error" });
            try
            {
                await request.SendAsync();
            }
            catch (GenesysMethodException e)
            {
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.HttpStatusCode);
                Assert.AreEqual(1, e.StatusCode);
                Assert.AreEqual("error", e.StatusMessage);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(GenesysMethodException))] // HttpRequestException does not return enough detail
        public async Task SendAsync_responds_with_500_Server_Error_but_statusCode_OK()
        {
            var request = client.CreateRequest("GET", "/foo");
            httpMock.setResponse(HttpStatusCode.InternalServerError, new { statusCode = 0 });
            await request.SendAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(GenesysMethodException))]
        public async Task SendAsync_responds_with_200_OK_but_errored_statusCode()
        {
            var request = client.CreateRequest("GET", "/foo");
            httpMock.setResponse(HttpStatusCode.OK, new { statusCode = 1 });
            await request.SendAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(GenesysMethodException))]
        public async Task SendAsync_responds_with_200_OK_but_empty_content()
        {
            var request = client.CreateRequest("GET", "/foo");
            httpMock.setResponse(HttpStatusCode.OK, null);
            await request.SendAsync();
        }


        // ok empty response
        // ok json response
        // not json response
        // json response empty
        // json response bad format


    }
}
