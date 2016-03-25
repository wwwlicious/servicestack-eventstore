namespace ServiceStack.EventStore.Subscriptions
{
    public class CatchUpSubscription : Subscription
    {
        public CatchUpSubscription(string streamId, string subscriptionGroup = default(string),int maxNoOfRetries = 5) 
                : base(streamId, subscriptionGroup, maxNoOfRetries) {}
    }
}
