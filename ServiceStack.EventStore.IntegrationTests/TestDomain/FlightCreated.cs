using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class FlightCreated : IDomainEvent
    {
        public FlightCreated(Guid id)
        {
            FlightId = id;
        }

        public Guid FlightId { get; }
    }
}