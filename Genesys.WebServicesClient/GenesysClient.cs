using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient
{
    public class GenesysClient : IDisposable
    {
        internal readonly Setup setup;
        internal readonly string credentials;

        readonly HttpClient httpClient;
        readonly bool disposeOfHttpClient;
        readonly string serverUri;
        readonly TimeSpan requestTimeout;

        //TODO readonly CookieSession cookieSession;
        //TODO readonly Authentication authentication;

        // It has been preferred to expose TaskScheduler instead of SynchronizationContext, as a TaskScheduler
        // is what is actually needed by HttpClient's async methods.
        // A TaskScheduler can be obtained from a SynchronizationContext by using TaskScheduler.FromCurrentSynchronizationContext().
        readonly TaskScheduler asyncTaskScheduler;

        protected GenesysClient(Setup setup)
        {
            this.setup = setup;
            this.httpClient = setup.httpClient;
            this.disposeOfHttpClient = setup.disposeOfHttpClient;
            this.serverUri = setup.serverUri;
            this.credentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(setup.UserName + ":" + setup.Password));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            this.requestTimeout = setup.RequestTimeout;
            //this.cookieSession = setup.cookieSession;
            //this.authentication = setup.authentication;
            this.asyncTaskScheduler = setup.AsyncTaskScheduler;
        }

        public void Dispose()
        {
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

        public GenesysEventReceiver CreateEventReceiver()
        {
            return new GenesysEventReceiver(this);
        }

        public HttpClient HttpClient
        {
            get { return httpClient; }
        }

        public class Setup
        {
            internal string serverUri;
            internal HttpClient httpClient;
            internal bool disposeOfHttpClient = true;
            //internal Authentication authentication;
            //internal CookieSession cookieSession = new CookieSessionImpl();
            public TimeSpan ConnectTimeout { get; set; }
            public TimeSpan RequestTimeout { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }

            public Setup()
            {
                RequestTimeout = TimeSpan.FromSeconds(5);
            }

            public GenesysClient Create()
            {
                //if (authentication == null)
                //    throw new InvalidOperationException(
                //        "Credentials (username and password) are mandatory. " +
                //        "If you want no credentials please use anonymous() explicitly.");

                if (httpClient == null)
                    httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri(serverUri),
                        Timeout = RequestTimeout,
                    };

                if (AsyncTaskScheduler == null)
                    try
                    {
                        AsyncTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    }
                    catch (InvalidOperationException)
                    {
                        AsyncTaskScheduler = null;
                    }
                    
                return new GenesysClient(this);
            }

            public string ServerUri
            {
                get
                {
                    return serverUri;
                }

                set
                {
                    if (value.EndsWith("/"))
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


            public bool Anonymous
            {
                set
                {
                    //if (value)
                    //    this.authentication = NoAuthentication.Instance;
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
