namespace ServiceStack.EventStore.Subscriptions
{
    public class PersistentSubscription : Subscription
    {

        public PersistentSubscription(string streamId, string subscriptionGroup, int maxNoOfRetries = 5) 
            : base(streamId, subscriptionGroup, maxNoOfRetries) { }
    }
}
