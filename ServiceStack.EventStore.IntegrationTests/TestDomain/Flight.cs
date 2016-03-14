namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using Types;

    public class Flight : Aggregate<FlightState>
    {
        public Flight(Guid id): base(id, new FlightState()) { }

        public static Flight CreateNew()
        {
            return new Flight(Guid.NewGuid());
        }

        public void ChangeFlightNumber(string newFlightNumber)
        {
            Causes(new FlightNumberChanged(newFlightNumber));
        }

        public void ChangeDestination(string destination)
        {
            Causes(new DestinationChanged(destination));
        }

        public void SetEstimatedDepartureTime(DateTime dateTime)
        {
            Causes(new EDTUpdated(dateTime));
        }
    }
}
