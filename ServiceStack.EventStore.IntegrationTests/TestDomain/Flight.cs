namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using Types;

    public class Flight : Aggregate<FlightState>
    {
        public Flight(Guid id) : base(id, new FlightState()) {}

        public static Flight CreateNew()
        {
            var flight = new Flight(Guid.NewGuid());
            flight.Causes(new FlightCreated(flight.Id));
            return flight;
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

        public void AddBaggageToHold(int noOfBags)
        {
            Causes(new BaggageAdded(noOfBags));
        }
    }
}
