using Cometd.Bayeux;
using Cometd.Client.Transport;
using Cometd.Common;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Genesys.WebServicesClient.Impl
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

    /// <summary>
    /// This is a translation of CometD Java WebSocketTransport. The translation has been kept as close as
    /// possible to the source, even when the code could have be improved.
    /// </summary>
    class WebSocketTransport : HttpClientTransport, IMessageClientTransport
    {
        private const string NAME = "websocket";
//    public final static String PREFIX = "ws";
//    public final static String PROTOCOL_OPTION = "protocol";
        private const string CONNECT_TIMEOUT_OPTION = "connectTimeout";
//    public final static String IDLE_TIMEOUT_OPTION = "idleTimeout";
//    public final static String MAX_MESSAGE_SIZE_OPTION = "maxMessageSize";
        private const String STICKY_RECONNECT_OPTION = "stickyReconnect";

//    public static WebSocketTransport create(Map<String, Object> options, WebSocketClientFactory webSocketClientFactory)
//    {
//        return create(options, webSocketClientFactory, null);
//    }

//    public static WebSocketTransport create(Map<String, Object> options, WebSocketClientFactory webSocketClientFactory, ScheduledExecutorService scheduler)
//    {
//        WebSocketTransport transport = new WebSocketTransport(options, webSocketClientFactory, scheduler);
//        if (!webSocketClientFactory.isStarted())
//        {
//            try
//            {
//                webSocketClientFactory.start();
//            }
//            catch (Exception x)
//            {
//                throw new RuntimeException(x);
//            }
//        }
//        return transport;
//    }

//    private final WebSocket _websocket = new CometDWebSocket();
        private readonly ConcurrentDictionary<String, WebSocketExchange> _exchanges = new ConcurrentDictionary<String, WebSocketExchange>();
//    private final WebSocketClientFactory _webSocketClientFactory;
//    private volatile ScheduledExecutorService _scheduler;
//    private volatile boolean _shutdownScheduler;
//    private volatile String _protocol = null;
        private volatile int _maxNetworkDelay;
        private volatile int _connectTimeout;
//    private volatile int _idleTimeout;
//    private volatile int _maxMessageSize;
        private volatile bool _stickyReconnect;
        private volatile bool _connected;
        private volatile bool _disconnected;
        private volatile bool _aborted;
        private volatile bool _webSocketSupported = true;
        private volatile bool _webSocketConnected = false;
        private volatile WebSocket _connection;
        private volatile ITransportListener _listener;
        private volatile IDictionary<string, object> _advice;

        private readonly List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();
        private readonly ManualResetEvent _openedEvent = new ManualResetEvent(false);
        private Exception _openedException;

        public WebSocketTransport(IDictionary<String, Object> options
            /*, WebSocketClientFactory webSocketClientFactory, ScheduledExecutorService scheduler*/)
            : base(NAME, options)
        {
            //this(null, options, webSocketClientFactory, scheduler);
        }

        public WebSocketTransport(IList<KeyValuePair<string, string>> headers)
            : base(NAME, null)
        {
            this._headers = headers.ToList();
        }
                
//    public WebSocketTransport(String url, Map<String, Object> options, WebSocketClientFactory webSocketClientFactory, ScheduledExecutorService scheduler)
//    {
//        super(NAME, url, options);
//        _webSocketClientFactory = webSocketClientFactory;
//        _scheduler = scheduler;
//        setOptionPrefix(PREFIX);
//    }

        public void setMessageTransportListener(ITransportListener listener)
        {
            _listener = listener;
        }

        public override bool accept(string version)
        {
            return _webSocketSupported;
        }

        override public void init()
        {
            base.init();
            _exchanges.Clear();
            _aborted = false;
//        _protocol = getOption(PROTOCOL_OPTION, _protocol);
            _maxNetworkDelay = getOption(MAX_NETWORK_DELAY_OPTION, 15000);
            _connectTimeout = getOption(CONNECT_TIMEOUT_OPTION, 30000);
//        _idleTimeout = getOption(IDLE_TIMEOUT_OPTION, 60000);
//        _maxMessageSize = getOption(MAX_MESSAGE_SIZE_OPTION, _webSocketClientFactory.getBufferSize());
            _stickyReconnect = getOption(STICKY_RECONNECT_OPTION, true);
//        if (_scheduler == null)
//        {
//            _shutdownScheduler = true;
//            _scheduler = Executors.newSingleThreadScheduledExecutor();
//        }
        }

        private int getMaxNetworkDelay()
        {
            return _maxNetworkDelay;
        }

        private int getConnectTimeout()
        {
            return _connectTimeout;
        }
        
        public override void abort()
        {
            _aborted = true;
            disconnect("Aborted");
//        shutdownScheduler();
        }

    // reset is the terminate in Java
//    public override void terminate()
        public override void reset()
        {
            disconnect("Terminated");
//        shutdownScheduler();
        }

//    private void shutdownScheduler()
//    {
//        if (_shutdownScheduler)
//        {
//            _shutdownScheduler = false;
//            _scheduler.shutdownNow();
//            _scheduler = null;
//        }
//    }

        protected void disconnect(string reason)
        {
            WebSocket connection = _connection;
            _connection = null;
            if (connection != null
                && (connection.State == WebSocketState.Connecting
                    || connection.State == WebSocketState.Open))
            {
                debug("Closing websocket connection '{0}' for reason '{1}'", connection, reason);
                connection.Close(1000 /* NormalClosure */, reason);
            }
        }

        public override void send(ITransportListener listener, IList<IMutableMessage> messages)
        {
            Trace.TraceInformation("send by {0}", listener);

            if (_aborted)
                throw new InvalidOperationException("Aborted");
        
            try
            {
                WebSocket connection = connect(listener, ObjectConverter.ToListOfIMessage(messages));
                if (connection == null)
                    return;

                foreach (IMutableMessage message in messages)
                    registerMessage(message, listener);

                var jsonParser = new System.Web.Script.Serialization.JavaScriptSerializer();
                String content = jsonParser.Serialize(ObjectConverter.ToListOfDictionary(messages));

                debug("Sending messages {0}", content);

                // The onSending() callback must be invoked before the actual send
                // otherwise we may have a race condition where the response is so
                // fast that it arrives before the onSending() is called.
                listener.onSending(ObjectConverter.ToListOfIMessage(messages));
                connection.Send(content);
            }
            catch (Exception x)
            {
                complete(messages);
                disconnect("Exception");
                listener.onException(x, ObjectConverter.ToListOfIMessage(messages));
            }
        }

        public override bool isSending
        {
            get { return !_exchanges.IsEmpty; }
        }

        WebSocket connect(ITransportListener listener, IList<IMessage> messages)
        {
            WebSocket connection = _connection;
            if (connection != null)
                return connection;

            try
            {
                // Mangle the URL
                string url = getURL();
                url = "ws" + url.Substring(4); // replaceFirst("^http", "ws");
                debug("Opening websocket connection to {0}", url);

                var cookies = new List<KeyValuePair<string, string>>();
                foreach (var cookieObj in getCookieCollection())
                {
                    var cookie = (System.Net.Cookie)cookieObj;
                    cookies.Add(new KeyValuePair<string,string>(cookie.Name, cookie.Value));
                }

                string subprotocol = "";
//            client.setProtocol(_protocol);
                WebSocket client = new WebSocket(url, subprotocol, cookies, _headers);
                client.Opened += onOpen;
                client.Error += client_Error;
                client.Closed += onClose;
                client.MessageReceived += onMessage;
            
                _openedEvent.Reset();
                Trace.TraceInformation("Thread {0}", Thread.CurrentThread.ManagedThreadId);
                client.Open();
                bool completed = _openedEvent.WaitOne(getConnectTimeout());
                if (!completed)
                    throw new TimeoutException("open");
                if (_openedException != null)
                    throw _openedException;

                _connection = client;
                // Connection was successful
                _webSocketConnected = true;

                if (_aborted)
                {
                    disconnect("Aborted");
                    listener.onException(new Exception("Aborted"), messages);
                }
            }
//        catch (ConnectException x)
//        {
//            listener.onConnectException(x, messages);
//        }
//        catch (UnresolvedAddressException x)
//        {
//            listener.onConnectException(x, messages);
//        }
//        catch (SocketTimeoutException x)
//        {
//            listener.onConnectException(x, messages);
//        }
//        catch (TimeoutException x)
//        {
//            listener.onConnectException(x, messages);
//        }
//        catch (InterruptedException x)
//        {
//            listener.onConnectException(x, messages);
//        }
//        catch (ProtocolException x)
//        {
//            // Probably a WebSocket upgrade failure
//            _webSocketSupported = false;
//            // Try to parse the HTTP error, although it's ugly
//            Map<String, Object> failure = new HashMap<String, Object>(2);
//            failure.put("websocketCode", 1002);
//            // Unfortunately the information on the HTTP status code is not available directly
//            // Try to parse it although it's ugly
//            Matcher matcher = Pattern.compile("(\\d+){3}").matcher(x.getMessage());
//            if (matcher.find())
//            {
//                int code = Integer.parseInt(matcher.group());
//                if (code > 100 && code < 600)
//                    failure.put("httpCode", code);
//            }
//            listener.onException(new TransportException(x, failure), messages);
//        }
            catch (Exception x)
            {
                _webSocketSupported = _stickyReconnect && _webSocketConnected;
                listener.onException(x, messages);
            }
            return _connection;
        }

//    protected WebSocketClient newWebSocketClient()
//    {
//        WebSocketClient result = _webSocketClientFactory.newWebSocketClient();
//        result.setMaxTextMessageSize(_maxMessageSize);
//        result.setMaxIdleTime(_idleTimeout);
//        return result;
//    }

        private void complete(IList<IMutableMessage> messages)
        {
            foreach (var message in messages)
                deregisterMessage(message);
        }

        private void registerMessage(IMutableMessage message, ITransportListener listener)
        {
            // Calculate max network delay
            int maxNetworkDelay = getMaxNetworkDelay();
            if (Channel_Fields.META_CONNECT == message.Channel)
            {
                IDictionary<string, object> advice = message.Advice;
                if (advice == null)
                    advice = _advice;
                if (advice != null)
                {
                    object timeout = advice["timeout"];
                    if (timeout is int)
                        maxNetworkDelay += (int)timeout;
                    else if (timeout is long)
                        maxNetworkDelay += (int)timeout;
                    else if (timeout != null)
                        maxNetworkDelay += int.Parse(timeout.ToString());
                }
                _connected = true;
            }

            // Schedule a task to expire if the maxNetworkDelay elapses
            //long expiration = TimeUnit.NANOSECONDS.toMillis(System.nanoTime()) + maxNetworkDelay;

            CancellationTokenSource cancelToken = new CancellationTokenSource();

            Task.Run(async delegate
            {
                await Task.Delay(maxNetworkDelay);

//                long now = TimeUnit.NANOSECONDS.toMillis(System.nanoTime());
//                long delay = now - expiration;
//                if (delay > 5000) // TODO: make the max delay a parameter ?
//                    debug("Message {} expired {} ms too late", message, delay);

                // Notify only if we won the race to deregister the message
                    WebSocketExchange exch = deregisterMessage(message);
                    if (exch != null /* && _webSocketClientFactory.isRunning() */)
                        onExpired(listener, message);
            }, cancelToken.Token);

        // Register the exchange
        // Message responses must have the same messageId as the requests

            WebSocketExchange exchange = new WebSocketExchange(message, listener, cancelToken);
            debug("Registering {0}", exchange);
            _exchanges[message.Id] = exchange;
//        Object existing = _exchanges.put(message.getId(), exchange);
//        // Paranoid check
//        if (existing != null)
//            throw new IllegalStateException();
        }

        protected void onExpired(ITransportListener listener, params IMessage[] messages)
        {
            listener.onExpire(messages);
        }

        private WebSocketExchange deregisterMessage(IMessage message)
        {
            WebSocketExchange exchange;
            _exchanges.TryRemove(message.Id, out exchange);
            if (Channel_Fields.META_CONNECT == message.Channel)
                _connected = false;
            else if (Channel_Fields.META_DISCONNECT == message.Channel)
                _disconnected = true;

            debug("Deregistering {0} for message {1}", exchange, message);

            if (exchange != null)
                exchange.cancelToken.Cancel(false);

            return exchange;
        }

        private bool isReply(IMessage message)
        {
            return message.Meta || isPublishReply(message);
        }

        private bool isPublishReply(IMessage message)
        {
            return message.ContainsKey(Message_Fields.SUCCESSFUL_FIELD);
        }

        private void failMessages(Exception cause)
        {
            foreach (WebSocketExchange exchange in _exchanges.Values) // Property Values is a read-only collection
            {
                IMutableMessage message = exchange.message;
                deregisterMessage(message);
                exchange.listener.onException(cause, new IMutableMessage[] { message });
            }
        }

        protected void onMessages(IList<IMutableMessage> messages)
        {
            foreach (IMutableMessage message in messages)
            {
                if (isReply(message))
                {
                    // Remembering the advice must be done before we notify listeners
                    // otherwise we risk that listeners send a connect message that does
                    // not take into account the timeout to calculate the maxNetworkDelay
                    if (Channel_Fields.META_CONNECT == message.Channel && message.Successful)
                    {
                        IDictionary<string, object> advice = message.Advice;
                        if (advice != null)
                        {
                            // Remember the advice so that we can properly calculate the max network delay
                            object timeout;
                            if (advice.TryGetValue("timeout", out timeout))
                                _advice = advice;
                        }
                    }

                    WebSocketExchange exchange = deregisterMessage(message);
                    if (exchange != null)
                    {
                        exchange.listener.onMessages(new IMutableMessage[] { message });
                    }
                    else
                    {
                        // If the exchange is missing, then the message has expired, and we do not notify
                        debug("Could not find request for reply {}", message);
                    }
                    if (_disconnected && !_connected)
                        disconnect("Disconnect");
                }
                else
                {
                    _listener.onMessages(new IMutableMessage[] { message });
                }
            }
        }

//    protected class CometDWebSocket implements WebSocket.OnTextMessage
//    {
        private void onOpen(object connection, EventArgs e)
        {
            debug("Opened websocket connection {0}. In thread {1}", connection, Thread.CurrentThread.ManagedThreadId);
            _openedException = null;
            _openedEvent.Set();
        }

        void client_Error(object sender, ErrorEventArgs e)
        {
            Trace.TraceError("WebSocket error: {0}. In thread {1}", e.Exception, Thread.CurrentThread.ManagedThreadId);
            _openedException = e.Exception;
            _openedEvent.Set();
        }

        //public void onClose(int closeCode, String message)
        private void onClose(object senderConnection, EventArgs e)
        {
            WebSocket connection = _connection;
            _connection = null;
    //        debug("Closed websocket connection with code {} {}: {} ", closeCode, message, connection);
            debug("Closed websocket connection {0} ", connection);
    //        failMessages(new EOFException("Connection closed " + closeCode + " " + message));
            failMessages(new Exception("Connection closed"));
        }


        private void onMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                IList<IMutableMessage> messages = DictionaryMessage.parseMessages(e.Message);
                debug("Received messages {0}", e.Message);
                onMessages(messages);
            }
            catch (Exception x)
            {
                failMessages(x);
                disconnect("Exception");
            }
        }
//    }

        private void debug(string message, params object[] args)
        {
            Trace.WriteLine(string.Format(message, args), "Cometd.WebSocketTransport");
        }

        private class WebSocketExchange
        {
            public readonly IMutableMessage message;
            public readonly ITransportListener listener;
//        private readonly ScheduledFuture<?> task;
            internal readonly CancellationTokenSource cancelToken;

            public WebSocketExchange(IMutableMessage message, ITransportListener listener, CancellationTokenSource cancelToken
                /*, ScheduledFuture<?> task*/)
            {
                this.message = message;
                this.listener = listener;
//            this.task = task;
                this.cancelToken = cancelToken;
            }

            public override string ToString()
            {
                return typeof(WebSocketExchange).Name + " " + message;
            }
        }
    }
}
