namespace ServiceStack.EventStore.Consumers
{
    public interface IEventConsumer
    {
        void ConnectToSubscription(string streamAggregate, string subscriptionGroup);
    }
}
