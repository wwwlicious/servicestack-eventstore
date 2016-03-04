using System;
using ServiceStack.EventStore.IntegrationTests.TestDomain;
using ServiceStack.EventStore.Repository;
using Xunit;
using Xunit.Abstractions;

namespace ServiceStack.EventStore.IntegrationTests
{
    public class Tests
    {
        private const string ListeningOn = "http://localhost:8088/";
        private TestAppHost appHost;
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper output;

        public Tests(ITestOutputHelper output)
        {
            this.output = output;

            appHost = new TestAppHost();
            appHost.Init();
            appHost.Start(ListeningOn);
            eventStore = appHost.Container.Resolve<IEventStoreRepository>();
        }

        [Fact]
        public void CanSaveTestAggregate()
        {
            var flight = Flight.CreateNew();

            flight.ChangeFlightNumber("CQH8821");
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(1));
            eventStore.Publish(flight);
        }

        //[Fact]
        //public void CanRehydrateTestAggregate()
        //{
            
        //}

    }
}
