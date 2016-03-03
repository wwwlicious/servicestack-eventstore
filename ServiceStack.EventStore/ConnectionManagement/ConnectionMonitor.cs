using EventStore.ClientAPI;

namespace ServiceStack.EventStore.ConnectionManagement
{
    using Logging;
    using System;

    public delegate void DisconnectedDelegate(object sender, ClientConnectionEventArgs args);
    public delegate void ConnectedDelegate(object sender, ClientConnectionEventArgs args);
    public delegate void AuthenticationFailedDelegate(object sender, ClientAuthenticationFailedEventArgs args);
    public delegate void ErrorOccurredDelegate(object sender, ClientErrorEventArgs args);
    public delegate void ReconnectingDelegate(object sender, ClientReconnectingEventArgs args);
    public delegate void ClosedDelegate(object sender, ClientClosedEventArgs args);

    public class ConnectionMonitor
    {
        private readonly ILog log;
        private readonly IEventStoreConnection connection;

        public ConnectionMonitor(IEventStoreConnection connection)
        {
            this.connection = connection;
            log = LogManager.GetLogger(GetType());
        }

        public DisconnectedDelegate OnDisconnected { get; set; }
        public ConnectedDelegate OnConnected { get; set; }
        public AuthenticationFailedDelegate OnAuthenticationFailed { get; set; }
        public ErrorOccurredDelegate OnErrorOccurred { get; set; }
        public ReconnectingDelegate OnReconnecting { get; set; }
        public ClosedDelegate OnClosed { get; set; }

        public void Configure()
        {
            connection.Connected += new EventHandler<ClientConnectionEventArgs>(OnConnected ?? DefaultOnConnected);
            connection.Disconnected += new EventHandler<ClientConnectionEventArgs>(OnDisconnected ?? DefaultOnDisconnected);
            connection.AuthenticationFailed += new EventHandler<ClientAuthenticationFailedEventArgs>(OnAuthenticationFailed ?? DefaultOnAuthenticationFailed);
            connection.ErrorOccurred += new EventHandler<ClientErrorEventArgs>(OnErrorOccurred ?? DefaultOnErrorOccurred);
            connection.Reconnecting += new EventHandler<ClientReconnectingEventArgs>(OnReconnecting ?? DefaultOnReconnecting);
            connection.Closed += new EventHandler<ClientClosedEventArgs>(OnClosed ?? DefaultOnClosed);
        }

        private void DefaultOnClosed(object sender, ClientClosedEventArgs args)
        {
            log.Warn($"Connection closed: {args.Reason}");
        }

        private void DefaultOnReconnecting(object sender, ClientReconnectingEventArgs args)
        {
            log.Info("Attempting to connect to EventStore");
        }

        private void DefaultOnErrorOccurred(object sender, ClientErrorEventArgs args)
        {
            log.Fatal("A connection error occurred");
        }

        private void DefaultOnAuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs args)
        {
            log.Fatal("Authentication failed");
        }

        private void DefaultOnDisconnected(object sender, ClientConnectionEventArgs args)
        {
            log.Warn("Disconnected from EventStore");
        }

        private void DefaultOnConnected(object sender, ClientConnectionEventArgs args)
        {
            log.Info("Connected to EventStore");
        }
    }
}
