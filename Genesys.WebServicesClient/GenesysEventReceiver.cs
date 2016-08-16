using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using Cometd.Client;
using Cometd.Client.Transport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient
{
    // Two Bayeux implementations have been evaluated for its use inside the Genesys Web Services Client Library:
    // - Oyatel's CometD implementation for .NET (https://github.com/Oyatel/CometD.NET)
    // - CodeTitans Libraries: http://codetitans.codeplex.com/
    //
    // CodeTitans Libraries offer a nice Bayeux implementation, but they don't seem to provide an easy way to
    // implement Bayeux on top of WebSockets.
    //
    // Oyatel's CometD are a translation of CometD Java to .NET. They don't offer a WebSocket implementation, but
    // it can be provided by translating the CometD Java WebSocketTransport.
    // 
    // This is the main reason Oyatel's libraries are chosen here.

    public class GenesysEventReceiver : IDisposable
    {
    	readonly ModifiedBayeuxClient bayeuxClient;
    	readonly TaskScheduler taskScheduler;
        readonly TaskFactory taskFactory;
        readonly ThreadSafe threadSafe;

        volatile IList<EventSubscription> subscriptions = new List<EventSubscription>();
	
        // Synchronize on bayeuxClient for access.
        bool isConnected = true;

        protected internal GenesysEventReceiver(GenesysClient client, Setup setup)
        {
            var headers = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("Authorization", "Basic " + client.encodedCredentials)
            };

            var transports = new List<ClientTransport>();

            if (setup.Transport != null)
            {
                transports.Add(setup.Transport);
            }
            else
            {
                if (setup.WebSocketsEnabled)
                    transports.Add(new WebSocketTransport(headers));

                transports.Add(new LongPollingTransport(null, req => req.Headers["Authorization"] = "Basic " + client.encodedCredentials));
            }

            if (setup.TransportDecorator != null)
                transports = transports.Select(setup.TransportDecorator).ToList();

            bayeuxClient = new ModifiedBayeuxClient(client.setup.ServerUri + "/api/v2/notifications", transports);

            if (client.setup.AsyncTaskScheduler == null)
                throw new Exception("AsyncTaskScheduler must have a value in order to create an event-receiver");
            this.taskScheduler = client.setup.AsyncTaskScheduler;

            this.taskFactory = new TaskFactory(taskScheduler);

            threadSafe = new ThreadSafe(bayeuxClient);
        }

        /// <summary>
        /// Opens the channel for receiving events. This method blocks until
        /// the channel is connected, and throws an exception if not connected
        /// successfully.
        /// 
        /// Calling <see cref="Dispose()"/> while this method is executing
        /// will make this method throw an <see cref="OpenFailedException"/>
        /// </summary>
        /// 
        /// <exception cref="OpenFailedException"/>
        /// <exception cref="TimeoutException"/>
        public void Open(int timeoutMs)
        {
            threadSafe.OpenImpl(timeoutMs, CancellationToken.None);
        }

        public async Task OpenAsync(int timeoutMs, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                bayeuxClient.disconnect();
            });

            await Task.Factory.StartNew(
                () => threadSafe.OpenImpl(timeoutMs, cancellationToken),
                TaskCreationOptions.LongRunning);
        }

        // Methods that can run in the background, in a worker thread.
        // They are put in a separate class in order to make sure that they don't access fields that are not prepared
        // for multithreaded access.
        class ThreadSafe
        {
        	readonly ModifiedBayeuxClient bayeuxClient;
    
            public ThreadSafe(ModifiedBayeuxClient bayeuxClient)
            {
                this.bayeuxClient = bayeuxClient;
            }

            public void OpenImpl(int timeoutMs, CancellationToken cancellationToken)
            {
                GTrace.Trace(GTrace.TraceType.Bayeux, "BayeuxClient handshaking...");
                bayeuxClient.handshake();
                BayeuxClient.State state = bayeuxClient.waitFor(timeoutMs,
                    new List<BayeuxClient.State>() {
                        // BayeuxClient.State.HANDSHAKING is the current state, we don't wait for it.
                        // BayeuxClient.State.CONNECTING is the successful next state, so we let it execute until CONNECTED.
                        BayeuxClient.State.UNCONNECTED,
                        BayeuxClient.State.REHANDSHAKING,
                        BayeuxClient.State.CONNECTED,
                        BayeuxClient.State.DISCONNECTING,
                        BayeuxClient.State.DISCONNECTED
                    });

                GTrace.Trace(GTrace.TraceType.Bayeux, "BayeuxClient state after handshake: " + state);

                switch (state)
                {
                    case BayeuxClient.State.CONNECTED: // success and finished.
                    case BayeuxClient.State.DISCONNECTING: // is received when disconnect was requested in the process.
                        break;

                    case BayeuxClient.State.UNCONNECTED: // is received for a failed connection, which will be retried by the BayeuxClient.
                    case BayeuxClient.State.REHANDSHAKING: // is received for a failed handshake, which will be retried by the BayeuxClient.
                    case BayeuxClient.State.DISCONNECTED: // failed
                        var ex = bayeuxClient.EraseLastException();
                        var causeOrBlank = ex == null ?
                            "" :
                            ": " + ex.Message;

                        if (ex is TimeoutException)
                        {
                            throw new TimeoutException("Handshake operation timed out" + causeOrBlank, ex);
                        }
                        else
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            throw new OpenFailedException("BayeuxClient failed to open" + causeOrBlank, ex);
                        }

                    case BayeuxClient.State.INVALID: // none of the awaited states were received
                        throw new TimeoutException("Handshake operation timed out");

                    default: // shouldn't happen
                        throw new Exception("Unexpected state received while waiting handshake: " + state);
                }
            }
        }

        /// <summary>
        /// Opens the channel for receiving events. This method just begins the open procedure.
        /// It does not block, so it does not guarantee that the channel is open when returning.
        /// </summary>
        public void BeginOpen()
        {
            GTrace.Trace(GTrace.TraceType.Bayeux, "BayeuxClient handshaking...");
            bayeuxClient.handshake();
        }

        /// <summary>
        /// Closes the receiving channel. This method just begins the close procedure.
        /// It does not block, so it does not guarantee that the channel is closed when returning.
        /// 
        /// This object may continue to be used after calling this method. <see cref="Open()"/> can
        /// be called again.
        /// </summary>
        public void Close()
        {
            bayeuxClient.disconnect();
        }

        public void Dispose()
        {
            Close();
        }

    //private void onConnected() {
    //    LOG.debug("Resubscribing all subscriptions");
    //    for (Subscription subscription : subscriptions) {
    //        bayeuxClient.getChannel(subscription.channel).subscribe(subscription.bayeuxListener);
    //    }
    //}
	    
        /// <summary>
        /// This method does not wait for a response to the subscribe request. Therefore, this method
        /// can be called without the danger of blocking for a long time.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
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
                GTrace.Trace(GTrace.TraceType.Event, "Received event. Channel {0}\n{1}",
                    message.Channel, GTrace.PrettifyJson(message.JSON));

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
                        // subscribe does not block for response. It ends calling CometD Transport.send.
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
        public class Setup
        {
            //    private final CookieSession cookieSession;
            //    private final Authentication authentication;
            //    private Executor eventExecutor;
            bool webSocketsEnabled = true;

            //public Setup(CookieSession session, Authentication authentication) {
            //{
                //        this.cookieSession = session;
                //        this.authentication = authentication;

                //        String webSocketProperty = System.getProperty("com.genesys.wsclient.websocket");
                //        this.webSocketEnabled = webSocketProperty == null
                //                ? true
                //                : Boolean.parseBoolean(webSocketProperty);
            //}

            //public GenesysEventReceiver Create()
            //{
                //        if (eventExecutor == null) {
                //            eventExecutor = client.asyncExecutor;
                //            if (eventExecutor == null) {
                //                throw new IllegalStateException("eventExecutor is mandatory");
                //            }
                //        }

            //    return new GenesysEventReceiver(this);
            //}

            //    /**
            //     * (Mandatory if GenesysClient asyncExecutor not set) Executor for handling events received.
            //     */
            //    public Setup eventExecutor(Executor eventExecutor) {
            //        this.eventExecutor = eventExecutor;
            //        return this;
            //    }

            public bool WebSocketsEnabled
            {
                get { return webSocketsEnabled; }
                set { webSocketsEnabled = value; }
            }

            public ClientTransport Transport { get; set; }

            public Func<ClientTransport, ClientTransport> TransportDecorator { get; set; }

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
        }

        class ModifiedBayeuxClient : BayeuxClient
        {
            public Exception lastException;

            public ModifiedBayeuxClient(String url, IList<ClientTransport> transports)
                : base(url, transports)
            { }

            public override void onFailure(Exception e, IList<IMessage> messages)
            {
                lastException = e;
                GTrace.Trace(GTrace.TraceType.BayeuxError, "BayeuxClient failure: {0}\n{1}", e, e.StackTrace);
            }

            public Exception EraseLastException()
            {
                var e = lastException;
                lastException = null;
                return e;
            }
        }
    }
}
