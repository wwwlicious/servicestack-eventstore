// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.ConnectionManagement
{
    using Logging;
    using System;
    using global::EventStore.ClientAPI;

    public delegate void OnDisconnectedDelegate(object sender, ClientConnectionEventArgs args);
    public delegate void OnConnectedDelegate(object sender, ClientConnectionEventArgs args);
    public delegate void OnAuthenticationFailedDelegate(object sender, ClientAuthenticationFailedEventArgs args);
    public delegate void OnErrorOccurredDelegate(object sender, ClientErrorEventArgs args);
    public delegate void OnReconnectingDelegate(object sender, ClientReconnectingEventArgs args);
    public delegate void OnClosedDelegate(object sender, ClientClosedEventArgs args);

    /// <summary>
    /// Monitors the connection to EventStore and exposes a number of delegates that the developer can override 
    /// to specify the behaviour of the system in response to an authentication failure etc.
    /// </summary>
    internal class ConnectionMonitor
    {
        private readonly ILog log;
        private readonly IEventStoreConnection connection;
        private readonly IMonitorSettings settings;

        public ConnectionMonitor(IEventStoreConnection connection, IMonitorSettings settings)
        {
            this.connection = connection;
            this.settings = settings;
            log = LogManager.GetLogger(GetType());
        }

        public void AddHandlers()
        {
            connection.Connected += new EventHandler<ClientConnectionEventArgs>(settings.OnConnected ?? DefaultOnConnected);
            connection.Disconnected += new EventHandler<ClientConnectionEventArgs>(settings.OnDisconnected ?? DefaultOnDisconnected);
            connection.AuthenticationFailed += new EventHandler<ClientAuthenticationFailedEventArgs>(settings.OnAuthenticationFailed ?? DefaultOnAuthenticationFailed);
            connection.ErrorOccurred += new EventHandler<ClientErrorEventArgs>(settings.OnErrorOccurred ?? DefaultOnErrorOccurred);
            connection.Reconnecting += new EventHandler<ClientReconnectingEventArgs>(settings.OnReconnecting ?? DefaultOnReconnecting);
            connection.Closed += new EventHandler<ClientClosedEventArgs>(settings.OnClosed ?? DefaultOnClosed);
        }

        private void DefaultOnClosed(object sender, ClientClosedEventArgs args) => 
            log.Warn($"Connection closed: {args.Reason}");

        private void DefaultOnReconnecting(object sender, ClientReconnectingEventArgs args) => 
            log.Info("Attempting to connect to EventStore");

        private void DefaultOnErrorOccurred(object sender, ClientErrorEventArgs args) => 
            log.Fatal("A connection error occurred");

        private void DefaultOnAuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs args) => 
            log.Fatal("Authentication failed");

        private void DefaultOnDisconnected(object sender, ClientConnectionEventArgs args) => 
            log.Warn("Disconnected from EventStore");

        private void DefaultOnConnected(object sender, ClientConnectionEventArgs args) => 
            log.Info("Connected to EventStore");
    }
}
