using System;
using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Types
{
    public class InvalidMessage : Event
    {
        public InvalidMessage(RecordedEvent originalEvent) : base("invalidmessages")
        {
            OriginalEventId = originalEvent.EventId;
            OriginalData = originalEvent.Data.FromAsciiBytes();
            OriginalMetadata = originalEvent.Metadata.FromAsciiBytes();
            OriginalEventStreamId = originalEvent.EventStreamId;
            OriginalEventNumber = originalEvent.EventNumber;
        }

        public string OriginalEventType { get; set; }
        public string OriginalData { get; }
        public string OriginalMetadata { get; }
        public string OriginalEventStreamId { get;  }
        public Guid OriginalEventId { get; }
        public int OriginalEventNumber { get; }
        public string InvalidityReason { get; }
    }
}