using EventStore.ClientAPI;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using FluentAssertions;
    using TestDomain;
    using Repository;
    using Xunit;
    using Exceptions;
    using Types;

    //NOTE: These integration tests require EventStore to be running locally!
    [Collection("AggregateTests")]
    public class AggregateTests : IClassFixture<ServiceStackHostFixture>
    {
        private readonly IEventStoreRepository eventStore;

        public AggregateTests(ServiceStackHostFixture fixture)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
        }

        [Fact]
        public void AnAggregateCanBeSavedAndRehydrated()
        {
            var flightToBePersisted = Flight.CreateNew();

            //Act
            flightToBePersisted.ChangeFlightNumber("CQH8821");
            flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(1));
            eventStore.Save(flightToBePersisted).Wait();
            var flightToBeRehydrated = eventStore.GetById<Flight>(flightToBePersisted.Id).Result;

            //Assert
            flightToBeRehydrated.Id.Should().Be(flightToBePersisted.Id);
        }

        [Fact]
        public void AggregateVersionIsCorrectAfterAddingEvents()
        {
            var flight = Flight.CreateNew();

            //Act
            flight.ChangeDestination("Malaga");
            //Assert
            flight.State.Version.Should().Be(2);

            //Act
            flight.ChangeFlightNumber("BA1987");
            //Assert
            flight.State.Version.Should().Be(3);
        }



        [Fact]
        public void AggregateStateIsCorrectAfterApplyingEvents()
        {
            //Arrange
            const string testDestination = "Malaga";
            const string testFlightNumber = "BA1987";

            var flight = Flight.CreateNew();

            //Act
            flight.ChangeDestination(testDestination);
            flight.ChangeFlightNumber(testFlightNumber);

            //Assert
            flight.State.Destination.Should().Be(testDestination);
            flight.State.FlightNumber.Should().Be(testFlightNumber);
        }

        [Fact]
        public void AggregateStateIsCorrectAfterRehydration()
        {
            const string originalDestination = "Hong Kong";
            const string originalFlightNumber = "BA1987";
            const string newDestination = "Barra";
            const string newFlightNumber = "BA1986";

            var newFlight = Flight.CreateNew();

            //Act
            newFlight.ChangeDestination(originalDestination);
            newFlight.ChangeFlightNumber(originalFlightNumber);

            eventStore.Save(newFlight).Wait();

            var firstRehydration = eventStore.GetById<Flight>(newFlight.Id).Result;

            firstRehydration.ChangeDestination(newDestination);
            firstRehydration.ChangeFlightNumber(newFlightNumber);

            eventStore.Save(firstRehydration).Wait();

            var secondRehydration = eventStore.GetById<Flight>(newFlight.Id).Result;

            //Assert
            secondRehydration.State.Destination.Should().Be(newDestination);
            secondRehydration.State.FlightNumber.Should().Be(newFlightNumber);
        }

        [Fact]
        public void RaisingAnAggregateEventThatIsNotHandledThrowsCorrectException()
        {
            var flight = Flight.CreateNew();

            Assert.Throws<NotImplementedException>(() => flight.AddBaggageToHold(3));
        }

        [Fact]
        public void ThrowsCorrectExceptionWhenLoadingNonPersistedAggregate()
        {
            Assert.ThrowsAsync<AggregateNotFoundException>(() => eventStore.GetById<Flight>(Guid.NewGuid())).Wait();
        }

        [Fact]
        public void ThrowsCorrectExceptionWhenLoadingSoftDeletedAggregate()
        {
            var flight = Flight.CreateNew();
            var streamName = StreamName(flight);

            //Act
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(23));
            eventStore.Save(flight).Wait();
            eventStore.Connection.DeleteStreamAsync(streamName, ExpectedVersion.Any).Wait();

            Assert.ThrowsAsync<AggregateNotFoundException>(() => eventStore.GetById<Flight>(flight.Id)).Wait();
        }

        [Fact]
        public void ThrowsCorrectExceptionWhenLoadingHardDeletedAggregate()
        {
            const bool hardDelete = true;
            var flight = Flight.CreateNew();
            var streamName = StreamName(flight);

            //Act
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(23));
            eventStore.Save(flight).Wait();
            eventStore.Connection.DeleteStreamAsync(streamName, ExpectedVersion.Any, hardDelete).Wait();

            Assert.ThrowsAsync<AggregateDeletedException>(() => eventStore.GetById<Flight>(flight.Id)).Wait();
        }

        [Fact]
        public void CorrectlyRetrievesSpecificVersion()
        {
            var flightToBePersisted = Flight.CreateNew();

            //Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.ChangeDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            eventStore.Save(flightToBePersisted).Wait();

            var rehydratedFlight = eventStore.GetById<Flight>(flightToBePersisted.Id, 5).Result;

            //Assert
            rehydratedFlight.State.Version.Should().Be(5);
            rehydratedFlight.State.Destination.Should().Be("Destination No: 3");
        }

        [Fact]
        public void CanRepeatedlyPersistAggregate()
        {
            var flightToBePersisted = Flight.CreateNew();

            //Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.ChangeDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            eventStore.Save(flightToBePersisted).Wait();

            for (var i = 11; i <= 20; i++)
            {
                flightToBePersisted.ChangeDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            eventStore.Save(flightToBePersisted).Wait();

        }

        private static string StreamName(Aggregate flight)
        {
            var streamName = $"{typeof (Flight).Name}-{flight.Id}";
            return streamName;
        }
    }
}
