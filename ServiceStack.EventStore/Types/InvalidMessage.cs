using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Consumers
{
    using Types;

    public class InvalidMessage : Event
    {
        private readonly RecordedEvent originalEvent;

        public InvalidMessage(RecordedEvent originalEvent) : base("")
        {
            this.originalEvent = originalEvent;
        }
    }
}