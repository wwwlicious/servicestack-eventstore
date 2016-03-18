using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class FlightNumberUpdated : IDomainEvent
    {
        public string NewFlightNumber { get; set; }

        public FlightNumberUpdated(string newFlightNumber)
        {
            NewFlightNumber = newFlightNumber;
        }
    }
}