namespace ServiceStack.EventStore.ConnectionManagement
{
    public interface IMonitorSettings
    {
        OnAuthenticationFailedDelegate OnAuthenticationFailed { get; set; }
        OnClosedDelegate OnClosed { get; set; }
        OnConnectedDelegate OnConnected { get; set; }
        OnDisconnectedDelegate OnDisconnected { get; set; }
        OnErrorOccurredDelegate OnErrorOccurred { get; set; }
        OnReconnectingDelegate OnReconnecting { get; set; }
    }
}