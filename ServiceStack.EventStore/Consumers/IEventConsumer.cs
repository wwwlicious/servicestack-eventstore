namespace ServiceStack.EventStore.Consumers
{
    using System.Threading.Tasks;

    public interface IEventConsumer
    {
        Task ConnectToSubscription(string streamName, string subscriptionGroup);
    }
}
