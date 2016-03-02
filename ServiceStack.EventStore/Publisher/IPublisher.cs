using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.Publisher
{
    public interface IPublisher
    {
        void Publish<TId>(AggregateEvent<TId> @event) where TId : struct;

    }
}