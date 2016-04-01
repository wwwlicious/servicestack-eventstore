namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using System.Collections.Generic;
    using Types;

    public class FlightState : StateManagement.State
    {
        public string FlightNumber { get; private set; }
        public IList<Passenger> Passengers { get; private set; }
        public string Destination { get; private set; }
        public int NoOfBagsInHold { get; private set; }
        public string Carrier { get; private set; }
        public DateTime ScheduledDepartureTime { get; private set; }
        public DateTime EstimatedDepartureTime { get; private set; }

        public void On(FlightCreated @event)
        {
            
        }

        public void On(FlightNumberUpdated @event)
        {
            FlightNumber = @event.NewFlightNumber;
        }

        public void On(PassengerAdded @event)
        {
            Passengers.Add(@event.Passenger);
        }

        public void On(DestinationChanged @event)
        {
            Destination = @event.Destination;
        }

        public void On(EDTUpdated @event)
        {
            EstimatedDepartureTime = @event.DepartureTime;
        }
    }
}
