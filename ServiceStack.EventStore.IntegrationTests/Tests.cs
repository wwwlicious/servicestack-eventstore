using System;
using FluentAssertions;
using ServiceStack.EventStore.IntegrationTests.TestDomain;
using ServiceStack.EventStore.Repository;
using Xunit;
using Xunit.Abstractions;

namespace ServiceStack.EventStore.IntegrationTests
{
    [Collection("AggregateTests")]
    public class Tests
    {
        private const string ListeningOn = "http://localhost:8088/";
        private TestAppHost appHost;
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper output;
        private Guid testAggregateId;

        public Tests(ITestOutputHelper output)
        {
            this.output = output;

            try
            {
                appHost = new TestAppHost();
                appHost.Init();
                appHost.Start(ListeningOn);
                eventStore = appHost.Container.Resolve<IEventStoreRepository>();
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message);
            }
        }

        [Fact]
        public void CanSaveAndRehydrateTestAggregate()
        {
            var flightToBePersisted = Flight.CreateNew();
       
            //Persist aggregate
            flightToBePersisted.ChangeFlightNumber("CQH8821");
            flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(1));
            eventStore.Publish(flightToBePersisted).Wait();

            //rehydrate aggregate
            var flightToBeRehydrated = eventStore.GetById<Flight>(flightToBePersisted.Id).Result;
            flightToBeRehydrated.Id.Should().Be(flightToBePersisted.Id);
        }

        [Fact]
        public void VersionIsCorrectAfterAddingEvents()
        {
            var flight = Flight.CreateNew();

            flight.ChangeDestination("Malaga");
            flight.State.Version.Should().Be(1);

            flight.ChangeFlightNumber("BA1987");
            flight.State.Version.Should().Be(2);
        }

        [Fact]
        public void StateIsCorrectAfterApplyingEvents()
        {
            const string testDestination = "Malaga";
            const string testFlightNumber = "BA1987";

            var flight = Flight.CreateNew();

            flight.ChangeDestination(testDestination);
            flight.State.Destination.Should().Be(testDestination);

            flight.ChangeFlightNumber(testFlightNumber);
            flight.State.FlightNumber.Should().Be(testFlightNumber);
        }

        ~Tests()
        {
            appHost.Dispose();
        }
    }
}
