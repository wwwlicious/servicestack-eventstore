namespace ServiceStack.EventStore.Types
{
    public interface IPublisher
    {
        void Publish<TEvent>(TEvent @event);

    }
}