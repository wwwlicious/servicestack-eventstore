using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class EDTUpdated : IDomainEvent
    {
        public DateTime DepartureTime { get; }

        public EDTUpdated(DateTime departureTime)
        {
            DepartureTime = departureTime;
        }
    }
}