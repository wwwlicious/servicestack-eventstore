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
