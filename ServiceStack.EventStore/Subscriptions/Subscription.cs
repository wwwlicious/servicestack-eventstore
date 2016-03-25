namespace ServiceStack.EventStore.Subscriptions
{
    public abstract class Subscription
    {
        protected Subscription(string streamId, string subscriptionGroup, int maxNoOfRetries)
        {
            StreamId = streamId;
            MaxNoOfRetries = maxNoOfRetries;
            SubscriptionGroup = subscriptionGroup;
        }

        public string StreamId { get; }
        public int MaxNoOfRetries  { get; }
        public string SubscriptionGroup { get; }
    }
}