namespace ServiceStack.EventStore.Subscriptions
{
    using Resilience;
    using System;
    using System.Collections.Generic;

    public abstract class Subscription
    {
        protected Subscription(string streamId, string subscriptionGroup)
        {
            StreamId = streamId;
            SubscriptionGroup = subscriptionGroup;
        }

        public string StreamId { get; }
        public string SubscriptionGroup { get; }
        public RetryPolicy RetryPolicy { get; set; }

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