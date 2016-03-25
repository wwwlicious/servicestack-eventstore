namespace ServiceStack.EventStore.Consumers
{
    using Resilience;
    using System.Threading.Tasks;

    public interface IEventConsumer
    {
        Task ConnectToSubscription(string streamId, string subscriptionGroup);
        void SetRetryPolicy(RetryPolicy retries);
    }
}
