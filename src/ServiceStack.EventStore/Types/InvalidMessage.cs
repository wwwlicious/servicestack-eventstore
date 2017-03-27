// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Types
{
    using System;
    using global::EventStore.ClientAPI;

    /// <summary>
    /// Represents a message that cannot be deserialized.
    /// </summary>
    internal class InvalidMessage 
    {
        public InvalidMessage(RecordedEvent originalEvent)
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
        public long OriginalEventNumber { get; }
        public string InvalidityReason { get; }
    }
}