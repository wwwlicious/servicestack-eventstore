using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests
{
    public class FlightNumberChanged : IDomainEvent
    {
        public Guid FlightId { get; }
        public string NewFlightNumber { get; }

        public FlightNumberChanged(Guid flightId, string newFlightNumber)
        {
            FlightId = flightId;
            NewFlightNumber = newFlightNumber;
        }
    }
}