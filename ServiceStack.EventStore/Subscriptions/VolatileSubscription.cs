namespace ServiceStack.EventStore.Subscriptions
{
    public class VolatileSubscription : Subscription
    {
        public VolatileSubscription(string streamId) : base(streamId, default(string)) { }
    }
}
