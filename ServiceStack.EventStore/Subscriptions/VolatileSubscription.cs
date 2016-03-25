namespace ServiceStack.EventStore.Subscriptions
{
    public class VolatileSubscription : Subscription
    {
        public VolatileSubscription(string streamId, string subscriptionGroup = default(string), int maxNoOfRetries = 5) 
            : base(streamId, subscriptionGroup, maxNoOfRetries) { }
    }
}
