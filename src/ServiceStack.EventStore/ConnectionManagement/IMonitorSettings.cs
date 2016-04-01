// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

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