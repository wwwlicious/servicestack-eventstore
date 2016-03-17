namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using Types;

    public class Flight : Aggregate<FlightState>
    {
        public Flight(Guid id) : base(id)
        {

        }

        public Flight(): base(Guid.NewGuid())
        {
            Causes(new FlightCreated(Id));
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
