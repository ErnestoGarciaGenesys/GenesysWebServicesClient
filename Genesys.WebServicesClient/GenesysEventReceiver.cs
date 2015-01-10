using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using Cometd.Client;
using Cometd.Client.Transport;
using Genesys.WebServicesClient.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient
{
    public class GenesysEventReceiver : IDisposable
    {
        static readonly string Namespace = typeof(GenesysEventReceiver).Namespace;
        static readonly TraceSource Log = new TraceSource(Namespace);
        static readonly TraceSource LogEvent = new TraceSource(Namespace + ".EVENT");
        static readonly TraceSource LogEventContentRaw = new TraceSource(Namespace + ".EVENT.CONTENT.RAW");
        static readonly TraceSource LogEventContentPretty = new TraceSource(Namespace + ".EVENT.CONTENT.PRETTY");
        static readonly TraceSource LogEventTransport = new TraceSource(Namespace + "EVENT.TRANSPORT");
        static readonly TraceSource LogEventTransportHeaders = new TraceSource(Namespace + ".EVENT.TRANSPORT.HEADERS");

    	readonly BayeuxClient bayeuxClient;
    	readonly TaskScheduler taskScheduler;
        readonly TaskFactory taskFactory;

        volatile IList<EventSubscription> subscriptions = new List<EventSubscription>();
	
        // Synchronize on bayeuxClient for access.
        bool isConnected = true;

        protected internal GenesysEventReceiver(GenesysClient client)
        {
            var headers = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("Authorization", "Basic " + client.credentials)
            };

            bayeuxClient = new ModifiedBayeuxClient(client.setup.ServerUri + "/api/v2/notifications",
                new List<ClientTransport>()
                {
                    new WebSocketTransport(headers),
                    new LongPollingTransportWithCredentials(null, client.credentials),
                });

            if (client.setup.AsyncTaskScheduler == null)
                throw new Exception("AsyncTaskScheduler must have a value in order to create an event-receiver");
            this.taskScheduler = client.setup.AsyncTaskScheduler;

            this.taskFactory = new TaskFactory(taskScheduler);
        }
        
	    public void Open(int timeoutMs)
        {
            Log.TraceInformation("BayeuxClient handshaking...");
            bayeuxClient.handshake();
            bayeuxClient.waitFor(timeoutMs, new List<BayeuxClient.State>() { BayeuxClient.State.CONNECTED });
            Log.TraceInformation(bayeuxClient.Connected ? "BayeuxClient connected" : "BayeuxClient not connected");
        }

        public void Dispose()
        {
            bayeuxClient.disconnect();
        }
	
    //private void onConnected() {
    //    LOG.debug("Resubscribing all subscriptions");
    //    for (Subscription subscription : subscriptions) {
    //        bayeuxClient.getChannel(subscription.channel).subscribe(subscription.bayeuxListener);
    //    }
    //}
	    
	    public IEventSubscription Subscribe(string channel, EventHandler<GenesysEvent> eventHandler)
        {
            var bayeuxListener = new MessageListener(taskFactory, eventHandler);
            return new EventSubscription(this, channel, bayeuxListener);
        }

        class MessageListener : IMessageListener
        {
            readonly TaskFactory taskFactory;
            readonly EventHandler<GenesysEvent> eventHandler;

            public MessageListener(TaskFactory taskFactory, EventHandler<GenesysEvent> eventHandler)
            {
                this.taskFactory = taskFactory;
                this.eventHandler = eventHandler;
            }

            public void onMessage(IClientSessionChannel channel, IMessage message)
            {
				LogEvent.TraceEvent(TraceEventType.Verbose, 1, "Received event on channel: %s", message.Channel);
                LogEventContentRaw.TraceEvent(TraceEventType.Verbose, 2, "Content: %s", message.JSON);
                //if (LogEventContentPretty.Switch.ShouldTrace(TraceEventType.Verbose))
                //    LogEventContentPretty.TraceEvent(TraceEventType.Verbose, 3, "Content:\n%s", JsonUtil.prettify(message.getJSON()));
                taskFactory.StartNew(() =>
                    {
                        eventHandler(this, new GenesysEvent(message));
                    });
            }
        }

        class EventSubscription : IEventSubscription
        {
            readonly GenesysEventReceiver eventReceiver;
            readonly string channel;
            readonly IMessageListener bayeuxListener;

            public EventSubscription(GenesysEventReceiver eventReceiver, string channel, IMessageListener bayeuxListener)
            {
                this.eventReceiver = eventReceiver;
                this.channel = channel;
                this.bayeuxListener = bayeuxListener;

                changeSubscriptions(s => s.Add(this));
			
                lock (eventReceiver.bayeuxClient)
                {
                    if (eventReceiver.isConnected)
                    {
                        eventReceiver.bayeuxClient.getChannel(channel).subscribe(bayeuxListener);
                    }
                }
			}
        
            public void Dispose()
            {
                changeSubscriptions(s => s.Remove(this));
			
			    lock (eventReceiver.bayeuxClient)
                {
				    if (eventReceiver.isConnected)
                    {
					    eventReceiver.bayeuxClient.getChannel(channel).unsubscribe(bayeuxListener);
				    }
			    }
            }

            void changeSubscriptions(Action<IList<EventSubscription>> action)
            {
                lock (eventReceiver.subscriptions)
                {
                    var newSubscriptions = new List<EventSubscription>(eventReceiver.subscriptions);
                    action(newSubscriptions);
                    eventReceiver.subscriptions = newSubscriptions;
                }
            }
        }

        public IEventSubscription SubscribeAll(EventHandler<GenesysEvent> eventHandler)
        {
            return Subscribe("/**", eventHandler);
        }
	
	
    //private static void logRequest(HttpExchange exchange) {
    //    String content;
    //    try {
    //        content = new String(exchange.getRequestContent().array(), "UTF-8");
    //    } catch (UnsupportedEncodingException e) {
    //        throw new RuntimeException(e);
    //    }
		
    //    LOG_EVENT_TRANSPORT.debug(
    //        "Comet request to server: " + exchange.getAddress() +
    //        ", " + exchange.getMethod() + " " + exchange.getRequestURI() +
    //        ", content: " + content);
		
    //    Jetty769Util.logHeaders(LOG_EVENT_TRANSPORT_HEADERS, exchange.getRequestFields());
    //}
	
    ///** 
    // * <p>The {@link Setup} class is not thread-safe. Therefore always do the whole setup
    // * and creation of a {@link GenesysEventReceiver} in the same thread, or use according
    // * multi-threading techniques.
    // */
    //public static class Setup {
    //    private final GenesysClient client;
    //    private final CookieSession cookieSession;
    //    private final Authentication authentication;
    //    private Executor eventExecutor;
    //    private boolean webSocketEnabled;

    //    protected Setup(GenesysClient client,
    //            CookieSession session, Authentication authentication) {
    //        this.client = client;
    //        this.cookieSession = session;
    //        this.authentication = authentication;
			
    //        String webSocketProperty = System.getProperty("com.genesys.wsclient.websocket");
    //        this.webSocketEnabled = webSocketProperty == null
    //                ? true
    //                : Boolean.parseBoolean(webSocketProperty);
    //    }
		
    //    public GenesysEventReceiver create() {
    //        if (eventExecutor == null) {
    //            eventExecutor = client.asyncExecutor;
    //            if (eventExecutor == null) {
    //                throw new IllegalStateException("eventExecutor is mandatory");
    //            }
    //        }
    //        return new GenesysEventReceiver(this);
    //    }

    //    /**
    //     * (Mandatory if GenesysClient asyncExecutor not set) Executor for handling events received.
    //     */
    //    public Setup eventExecutor(Executor eventExecutor) {
    //        this.eventExecutor = eventExecutor;
    //        return this;
    //    }
		
    //    /**
    //     * (Optional) Disable the use of WebSocket.
    //     * 
    //     * <p>By default, WebSocket is the preferred
    //     * transport layer for events, and HTTP long-polling is used as a fall-back.
    //     * 
    //     * <p>WebSocket can also be disabled by setting the system property
    //     * <code>com.genesys.wsclient.websocket=false</code>.
    //     */
    //    public Setup disableWebSocket() {
    //        webSocketEnabled = false;
    //        return this;
    //    }

        class ModifiedBayeuxClient : BayeuxClient
        {
            public ModifiedBayeuxClient(String url, IList<ClientTransport> transports)
                : base(url, transports)
            { }

            public override void onFailure(Exception e, IList<IMessage> messages)
            {
                Log.TraceEvent(TraceEventType.Error, 10, "BayeuxClient failure: %s\n%s", e, e.StackTrace);
            }
        }

        class LongPollingTransportWithCredentials : CustomizableLongPollingTransport
        {
            readonly string credentials;

            public LongPollingTransportWithCredentials(IDictionary<String, Object> options, string credentials) : base(options)
            {
                this.credentials = credentials;
            }

            public override void customize(System.Net.HttpWebRequest request)
            {
                base.customize(request);
                request.Headers["Authorization"] = "Basic " + credentials;
            }
        }


    }
}
