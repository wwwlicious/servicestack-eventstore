namespace ServiceStack.EventStore.Repository
{
    using Types;

    public interface IEventStore
    {
        void Publish(IAggregate aggregate);

        void Publish(Event @event);
    }
}