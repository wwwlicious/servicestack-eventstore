using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    internal class EDTUpdated : IDomainEvent
    {
        public DateTime DepartureTime { get; }
        public Guid Id { get; }

        public EDTUpdated(Guid id, DateTime departureTime)
        {
            Id = id;
            DepartureTime = departureTime;
        }
    }
}