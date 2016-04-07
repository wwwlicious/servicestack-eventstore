// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Subscriptions
{
    using Resilience;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a subscription to EventStore incl. StreamId, SubscriptionGroup and RetryPolicy
    /// </summary>
    public abstract class Subscription
    {
        protected Subscription(string streamId, string subscriptionGroup)
        {
            StreamId = streamId;
            SubscriptionGroup = subscriptionGroup;
        }

        internal string StreamId { get; }
        internal string SubscriptionGroup { get; }
        internal RetryPolicy RetryPolicy { get; set; }

        public Subscription SetRetryPolicy(IEnumerable<TimeSpan> durations)
        {
            RetryPolicy = new RetryPolicy(durations);
            return this;
        }

        public Subscription SetRetryPolicy(Retries maxNoOfRetries, Func<int, TimeSpan> provider)
        {
            RetryPolicy = new RetryPolicy(maxNoOfRetries, provider);
            return this;
        }
    }
}