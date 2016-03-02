namespace ServiceStack.EventStore.Types
{
    public interface IPublisher
    {
        void Publish(Event @event);

    }
}