using System;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class Passenger
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}