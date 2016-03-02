namespace ServiceStack.EventStore.Publisher
{
    using Consumers;
    using Types;

    public class EventPublisher: IPublisher
    {
        public void Publish<TEvent>(TEvent @event)
        {
            throw new System.NotImplementedException();
        }
    }
}
