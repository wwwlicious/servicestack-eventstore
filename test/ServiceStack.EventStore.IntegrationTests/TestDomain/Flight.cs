// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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

        public void UpdateFlightNumber(string newFlightNumber)
        {
            Causes(new FlightNumberUpdated(newFlightNumber));
        }

        public void UpdateDestination(string destination)
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
