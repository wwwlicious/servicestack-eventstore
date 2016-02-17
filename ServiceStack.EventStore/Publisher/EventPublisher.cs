using EventStore.ClientAPI;
using ServiceStack.EventStore.Consumers;

namespace ServiceStack.EventStore.Publisher
{
    using Types;
    using System;

    public class EventPublisher: IPublisher
    {
        private readonly IEventStoreConnection connection;

        public EventPublisher(IEventStoreConnection connection)
        {
            this.connection = connection;
        }
        public async void Publish(EventStoreEvent @event) 
        {
            var body = @event.ToJson();
            var metadata = @event.GetMetadata().ToJson();

            await connection.AppendToStreamAsync("emails", ExpectedVersion.Any, 
                                                    new EventData(Guid.NewGuid(), @event.GetType().Name,
                                                                true, body.ToAsciiBytes(), metadata.ToAsciiBytes()));
        }

        public async void Publisher(InvalidMessage message)
        {
            var body = message.ToJson();
            var metadata = message.GetMetadata().ToJson();

            await connection.AppendToStreamAsync("invalid-messages", ExpectedVersion.Any,
                                                    new EventData(Guid.NewGuid(), message.GetType().Name,
                                                                true, body.ToAsciiBytes(), metadata.ToAsciiBytes()));
        }
    }
}
