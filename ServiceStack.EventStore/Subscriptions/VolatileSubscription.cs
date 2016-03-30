namespace ServiceStack.EventStore.Subscriptions
{

    /// <summary>
    /// Represents a volatile subscription to EventStore.
    /// </summary>
    public class VolatileSubscription : Subscription
    {
        public VolatileSubscription(string streamId) : base(streamId, default(string)) { }
    }
}
