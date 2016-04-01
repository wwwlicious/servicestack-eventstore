// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

namespace ServiceStack.EventStore.Subscriptions
{
    /// <summary>
    /// Represents a catch-up subscription to EventStore for populating a read model
    /// </summary>
    public class ReadModelSubscription : Subscription
    {
        public ReadModelSubscription() 
            : base(string.Empty, string.Empty) { }
    }
}
