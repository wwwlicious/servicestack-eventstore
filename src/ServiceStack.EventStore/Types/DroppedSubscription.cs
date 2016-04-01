// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

namespace ServiceStack.EventStore.Types
{
    using global::EventStore.ClientAPI;
    using Resilience;
    using Subscriptions;

    /// <summary>
    /// Represents a subscription to EventStore that has been dropped that is used by the DroppedSubscriptionPolicy to handle it.
    /// </summary>
    internal class DroppedSubscription
    {
        public DroppedSubscription(Subscription subscription, string exceptionMessage, SubscriptionDropReason dropReason)
        {
            StreamId = subscription.StreamId;
            ExceptionMessage = exceptionMessage;
            DropReason = dropReason;
            RetryPolicy = subscription.RetryPolicy;
        }

        public string StreamId { get; }
        public string ExceptionMessage { get; }
        public SubscriptionDropReason DropReason { get; }
        public RetryPolicy RetryPolicy { get; }
    }
}
