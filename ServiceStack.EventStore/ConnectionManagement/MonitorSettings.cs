namespace ServiceStack.EventStore.ConnectionManagement
{
    public class MonitorSettings : IMonitorSettings
    {
        public OnDisconnectedDelegate OnDisconnected { get; set; }
        public OnConnectedDelegate OnConnected { get; set; }
        public OnClosedDelegate OnClosed { get; set; }
        public OnAuthenticationFailedDelegate OnAuthenticationFailed { get; set; }
        public OnReconnectingDelegate OnReconnecting { get; set; }
        public OnErrorOccurredDelegate OnErrorOccurred { get; set; }
    }
}
