namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Types;

    public class Flight : EventSourcedAggregate
    {
        private IList<Passenger> passengers;
        private string destination;
        private int noOfBagsInHold;
        private string carrier;
        private string flightNumber;
        private DateTime scheduledDepartureTime;
        private DateTime estimatedDepartureTime ;

        private Flight(Guid id): base(id)
        {
        }

        public static Flight CreateNew()
        {
            return new Flight(Guid.NewGuid());
        }

        public void ChangeFlightNumber(string newFlightNumber)
        {
            flightNumber = newFlightNumber;
            Causes(new FlightNumberChanged(Id, newFlightNumber));
        }

        public void SetEstimatedDepartureTime(DateTime dateTime)
        {
            estimatedDepartureTime = dateTime;
            Causes(new EDTUpdated(Id, dateTime));
        }

        public override void ApplyEvent(IDomainEvent @event)
        {
            When((dynamic) @event);
            Version++;
        }

        private void When(PassengerAdded passengerAdded)
        {
            passengers.Add(passengerAdded.Passenger);
            noOfBagsInHold += passengerAdded.NoOfBags;
        }

        private void When(FlightNumberChanged flightNumberChanged)
        {
            
        }

        private void When(EDTUpdated edtUpdated)
        {

        }

        private void Causes(IDomainEvent @event)
        {
            Changes.Add(@event);
            ApplyEvent(@event);
        }

        public override ICollection GetUncommittedEvents()
        {
            throw new NotImplementedException();
        }

        public override IMemento GetSnapshot()
        {
            throw new NotImplementedException();
        }
    }
}
