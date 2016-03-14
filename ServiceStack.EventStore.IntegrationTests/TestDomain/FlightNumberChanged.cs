using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class FlightNumberChanged : IDomainEvent
    {
        public string NewFlightNumber { get; }

        public FlightNumberChanged(string newFlightNumber)
        {
            NewFlightNumber = newFlightNumber;
        }
    }
}