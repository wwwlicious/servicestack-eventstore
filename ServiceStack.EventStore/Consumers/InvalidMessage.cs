using EventStore.ClientAPI;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.Consumers
{
    public class InvalidMessage : EventStoreEvent
    {
        private readonly RecordedEvent originalEvent;

        public InvalidMessage(RecordedEvent originalEvent)
        {
            this.originalEvent = originalEvent;
        }
    }
}