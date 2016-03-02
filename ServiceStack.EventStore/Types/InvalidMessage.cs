using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Types
{
    using System;

    public class InvalidMessage : AggregateEvent<Guid>
    {
        private readonly RecordedEvent originalEvent;

        public InvalidMessage(RecordedEvent originalEvent) : base("invalid-messages", Guid.NewGuid())
        {
            this.originalEvent = originalEvent;
        }
    }
}