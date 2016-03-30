namespace ServiceStack.EventStore.Subscriptions
{
    /// <summary>
    /// Represents a catch-up subscription to EventStore for populating a read model
    /// </summary>
    public class ReadModelSubscription : Subscription
    {
        public ReadModelSubscription() 
            : base(string.Empty, string.Empty) { }
    }
}
