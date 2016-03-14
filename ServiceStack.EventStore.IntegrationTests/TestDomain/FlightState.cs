using System;
using System.Collections.Generic;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class FlightState : State
    {
        public string FlightNumber { get; private set; }
        public IList<Passenger> Passengers { get; }
        public string Destination { get; private set; }

        private int noOfBagsInHold;
        private string carrier;
        private DateTime scheduledDepartureTime;
        private DateTime estimatedDepartureTime;

        public void On(FlightNumberChanged @event)
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
            estimatedDepartureTime = @event.DepartureTime;
        }
    }
}
