using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient
{
    // TODO Support cookies
    public class GenesysClient : IDisposable
    {
        internal readonly Setup setup;
        internal readonly string encodedCredentials;

        readonly HttpClient httpClient;
        readonly bool disposeOfHttpClient;
        readonly string serverUri;
        readonly TimeSpan requestTimeout;

        bool disposed = false;

        //TODO readonly CookieSession cookieSession;
        //TODO readonly Authentication authentication;

        // DESIGN: Use of TaskScheduler instead of SynchronizationContext
        // Preferred as a TaskScheduler is what is actually needed by HttpClient's async methods.
        // A TaskScheduler can be obtained from a SynchronizationContext by using TaskScheduler.FromCurrentSynchronizationContext().
        readonly TaskScheduler asyncTaskScheduler;

        // DESIGN: Use of the Builder pattern
        // Builder pattern is being used here for the big number of parameters the constructor would need.
        // It also makes it easier to expand with more optional parameters, and keep backwards compatible.
        // Builder is used here under the name Setup, which has a clearer meaning. We could change it to Builder if it is more familiar to clients.
        protected GenesysClient(Setup setup)
        {
            this.setup = setup;
            this.httpClient = setup.httpClient;
            this.disposeOfHttpClient = setup.disposeOfHttpClient;
            this.serverUri = setup.serverUri;
            this.encodedCredentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(setup.UserName + ":" + setup.Password));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            this.requestTimeout = setup.RequestTimeout;
            //this.cookieSession = setup.cookieSession;
            //this.authentication = setup.authentication;
            this.asyncTaskScheduler = setup.AsyncTaskScheduler;
        }

        public void Dispose()
        {
            disposed = true;

            if (disposeOfHttpClient)
                httpClient.Dispose();
        }

        public GenesysRequest CreateRequest(string httpMethod, string uri, object jsonContent)
        {
            string absoluteUri;
            if (uri.StartsWith("/"))
                absoluteUri = serverUri + uri;
            else if (uri.StartsWith("http"))
                absoluteUri = uri;
            else
                throw new ArgumentException(
                        "URI must be either complete (starting with \"http\"),"
                        + " or an absolute path ((starting with \"/\"): " + uri);

            return new GenesysRequest(this, httpMethod, absoluteUri, jsonContent);
        }

        public GenesysRequest CreateRequest(string httpMethod, string uri)
        {
            return CreateRequest(httpMethod, uri, null);
        }

        public GenesysEventReceiver CreateEventReceiver(GenesysEventReceiver.Setup setup)
        {
            return new GenesysEventReceiver(this, setup);
        }

        public HttpClient HttpClient
        {
            get { return httpClient; }
        }

        public bool Disposed
        {
            get { return disposed; }
        }

        // DESIGN: Use of Properties instead of a Fluent Construction
        // For 2 reasons:
        // - C# Object Initializers make a good syntax already
        // - Fluent interfaces should be kept for very frequent use. Not the case here
        public class Setup
        {
            internal string serverUri;
            internal HttpClient httpClient;
            internal bool disposeOfHttpClient = true;
            //internal Authentication authentication;
            //internal CookieSession cookieSession = new CookieSessionImpl();
            //public TimeSpan ConnectTimeout { get; set; }
            public TimeSpan RequestTimeout { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool Anonymous { get; set; }

            public Setup()
            {
                RequestTimeout = TimeSpan.FromSeconds(5);
            }

            public GenesysClient Create()
            {
                if ((UserName == null || Password == null) & !Anonymous)
                    throw new InvalidOperationException(
                        "UserName and Password are mandatory. " +
                        "If you want no credentials please set Anonymous explicitly.");

                if (httpClient == null)
                {
                    if (serverUri == null)
                        throw new InvalidOperationException("Either property ServerUri or HttpClient is mandatory");

                    httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri(serverUri),
                        Timeout = RequestTimeout,
                    };
                }

                if (AsyncTaskScheduler == null)
                {
                    try
                    {
                        AsyncTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    }
                    catch (InvalidOperationException)
                    {
                        AsyncTaskScheduler = null;
                    }
                }
                    
                return new GenesysClient(this);
            }

            public string ServerUri
            {
                internal get
                {
                    return serverUri;
                }

                set
                {
                    if (value != null && value.EndsWith("/"))
                        throw new ArgumentException("Server URI must not end with a slash '/'");
                    
                    serverUri = value;
                }
            }

            public HttpClient HttpClient
            {
                set
                {
                    this.disposeOfHttpClient = value == null;
                    this.httpClient = value;
                }
            }

            //public bool CookiesEnabled
            //{
            //    set
            //    {
            //        if (!value)
            //            this.cookieSession = NoCookieSession.Instance;
            //    }
            //}

            public TaskScheduler AsyncTaskScheduler { set; internal get;  }

            public GenesysClient SharedHttpClient
            {
                set
                {
                    this.disposeOfHttpClient = false;
                    HttpClient = value.httpClient;
                }
            }

            //public GenesysClient SharedCookies
            //{
            //    set
            //    {
            //        this.cookieSession = value.cookieSession;
            //    }
            //}
        }

    }
}
