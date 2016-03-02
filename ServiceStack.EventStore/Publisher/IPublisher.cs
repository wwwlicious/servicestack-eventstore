namespace ServiceStack.EventStore.Publisher
{
    using Types;

    public interface IPublisher
    {
        void Publish<TId>(AggregateEvent<TId> @event) where TId : struct;

    }
}