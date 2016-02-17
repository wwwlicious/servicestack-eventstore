using System;
using EventStore.ClientAPI;
using ServiceStack.Logging;

namespace ServiceStack.EventStore.ConnectionManagement
{
    public class ConnectionMonitor
    {
        private readonly ILog log;
        private readonly IEventStoreConnection connection;

        public ConnectionMonitor(IEventStoreConnection connection)
        {
            this.connection = connection;
            log = LogManager.GetLogger(GetType());
        }

        public void Configure()
        {
            connection.Connected += OnConnected;
            connection.Disconnected += OnDisconnected;
            connection.AuthenticationFailed += OnAuthenticationFailed;
            connection.ErrorOccurred += OnErrorOccurred;
            connection.Reconnecting += OnReconnecting;
            connection.Closed += OnClosed;
        }

        public Type GetConnectionType()
        {
            return connection.GetType();
        }

        private void OnClosed(object sender, ClientClosedEventArgs clientClosedEventArgs)
        {
            log.Warn("Connection closed");
        }

        private void OnReconnecting(object sender, ClientReconnectingEventArgs clientReconnectingEventArgs)
        {
            log.Info("Attempting to connect to EventStore");
        }

        private void OnErrorOccurred(object sender, ClientErrorEventArgs clientErrorEventArgs)
        {
            log.Fatal("A connection error occurred");
        }

        private void OnAuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs clientAuthenticationFailedEventArgs)
        {
            log.Fatal("Authentication failed");
        }

        private void OnDisconnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            log.Warn("Disconnected from EventStore");
        }

        private void OnConnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            log.Info("Connected to EventStore");
        }
    }
}
