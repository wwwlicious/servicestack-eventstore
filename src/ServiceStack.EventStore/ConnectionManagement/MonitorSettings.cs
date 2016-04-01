// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

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
