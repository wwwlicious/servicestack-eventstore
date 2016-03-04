namespace ServiceStack.EventStore.IntegrationTests
{
    using System;
    using Types;

    public class FlightCreated : IDomainEvent
    {
        public FlightCreated(Guid id)
        {
            FlightId = id;
        }

        public Guid FlightId { get; }
    }
}