namespace ServiceStack.EventStore.Subscriptions
{
    public class CatchUpSubscription : Subscription
    {
        public CatchUpSubscription(string streamId) : base(streamId, default(string)) {}
    }
}
