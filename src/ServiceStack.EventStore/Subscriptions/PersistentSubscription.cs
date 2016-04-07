// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Subscriptions
{
    /// <summary>
    /// Represents a persistent subscription to EventSTore.
    /// </summary>
    public class PersistentSubscription : Subscription
    {
        public PersistentSubscription(string streamId, string subscriptionGroup)
            : base(streamId, subscriptionGroup) { }

    }
}
