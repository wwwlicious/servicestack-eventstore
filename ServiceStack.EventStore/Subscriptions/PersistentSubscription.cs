namespace ServiceStack.EventStore.Subscriptions
{
    public class PersistentSubscription : Subscription
    {

        public PersistentSubscription(string streamId, string subscriptionGroup)
            : base(streamId, subscriptionGroup) { }

    }
}
