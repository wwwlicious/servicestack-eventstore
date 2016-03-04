namespace ServiceStack.EventStore.Repository
{
    using Types;

    public interface IEventStoreRepository
    {
        void Publish(EventSourcedAggregate eventSourcedAggregate);

        void Publish(Event @event);
    }
}