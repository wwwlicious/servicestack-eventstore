namespace ServiceStack.EventStore.Types
{
    public interface IPublisher
    {
        void Publish(EventStoreEvent @event);
    }
}