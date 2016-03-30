namespace ServiceStack.EventStore.Subscriptions
{
    /// <summary>
    /// Represents a catch-up subscription to EventStore.
    /// </summary>
    public class CatchUpSubscription : Subscription
    {
        public CatchUpSubscription(string streamId) : base(streamId, default(string)) {}
    }
}
