namespace ServiceStack.EventStore.Consumers
{
    public interface IEventConsumer
    {
        void ConnectToSubscription(string streamName, string subscriptionGroup);
    }
}
