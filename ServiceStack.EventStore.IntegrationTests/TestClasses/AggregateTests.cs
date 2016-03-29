namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using FluentAssertions;
    using TestDomain;
    using Repository;
    using Xunit;
    using Exceptions;
    using Types;
    using Extensions;
    using Xunit.Abstractions;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;

    //NOTE: These integration tests require EventStore to be running locally!
    [Collection("ServiceStackHostCollection")]
    [Trait("Category", "Integration")]
    public class AggregateTests
    {
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper testOutput;

        public AggregateTests(ServiceStackHostFixture fixture, ITestOutputHelper output)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            testOutput = output;
        }

        [Fact]
        public async Task AnAggregateCanBeSavedAndRehydrated()
        {
            var flightToBePersisted = new Flight();

            //Act
            flightToBePersisted.UpdateFlightNumber("CQH8821");
            flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(1));

            await eventStore.SaveAsync(flightToBePersisted);

            var flightToBeRehydrated = await eventStore.GetByIdAsync<Flight>(flightToBePersisted.Id);

            //Assert
            flightToBeRehydrated.Id.Should().Be(flightToBePersisted.Id);
        }

        [Fact]
        public void AggregateVersionIsCorrectAfterAddingEvents()
        {
            var flight = new Flight();

            //Act
            flight.UpdateDestination("Malaga");
            //Assert
            flight.State.Version.Should().Be(2);

            //Act
            flight.UpdateFlightNumber("BA1987");
            //Assert
            flight.State.Version.Should().Be(3);
        }

        [Fact]
        public void AggregateStateIsCorrectAfterApplyingEvents()
        {
            //Arrange
            const string testDestination = "Malaga";
            const string testFlightNumber = "BA1987";

            var flight = new Flight();

            //Act
            flight.UpdateDestination(testDestination);
            flight.UpdateFlightNumber(testFlightNumber);

            //Assert
            flight.State.Destination.Should().Be(testDestination);
            flight.State.FlightNumber.Should().Be(testFlightNumber);
        }

        [Fact]
        public async Task GetsEventsFromCorrectStreams()
        {
            var firstFlight = new Flight();
            var secondFlight = new Flight();

            const string firstFlightNumber = "BA7651";
            const string secondlightNumber = "BA7652";

            firstFlight.UpdateFlightNumber(firstFlightNumber);
            secondFlight.UpdateFlightNumber(secondlightNumber);

            testOutput.WriteLine($"First Flight Id: {firstFlight.Id}");
            testOutput.WriteLine($"Second Flight Id: {secondFlight.Id}");

            await eventStore.SaveAsync(firstFlight);
            await eventStore.SaveAsync(secondFlight);

            var firstSaved = await eventStore.GetByIdAsync<Flight>(firstFlight.Id);
            var secondSaved = await eventStore.GetByIdAsync<Flight>(secondFlight.Id);

            firstSaved.State.FlightNumber.Should().Be(firstFlightNumber);
            secondSaved.State.FlightNumber.Should().Be(secondlightNumber);
        }

        [Fact]
        public async Task ThrowsOnRequestingSpecificVersionHigherThanExists()
        {
            var flightToBePersisted = new Flight();
            flightToBePersisted.UpdateDestination("Grimthorpe");
            flightToBePersisted.UpdateFlightNumber("XY6876");
            flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(134));

            await eventStore.SaveAsync(flightToBePersisted);

            var correctVersion = flightToBePersisted.State.Version;
            var incorrectVersion = correctVersion.Add(1);

            await Assert.ThrowsAsync<AggregateVersionException>(() => eventStore.GetByIdAsync<Flight>(flightToBePersisted.Id, incorrectVersion));
        }

        //todo: currently no way of reading additional headers (other than CLR type) from events
        [Fact]
        public async Task CanAddHeadersToAggregateEvents()
        {
            var flight = new Flight();

            flight.UpdateDestination("La Palma");
            flight.UpdateFlightNumber("LT7987");
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(67));

            await eventStore.SaveAsync(flight, headers =>
            {
                headers.Add("User", "Lil'Joey Brown");
                headers.Add("Other", "SomeImportantInfo");
            });

            await eventStore.GetByIdAsync<Flight>(flight.Id);
        }

        [Fact]
        public async Task CanSaveAndRehydrateLargeNumberOfEvents()
        {
            const int noOfEvents = 5000;

            var newFlight = new Flight();

            do
            {
                newFlight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(1));
            }
            while (newFlight.State.Version < noOfEvents);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await eventStore.SaveAsync(newFlight);

            var timeToSave = stopWatch.Elapsed;
            testOutput.WriteLine($"{noOfEvents} events written in {timeToSave}");

            stopWatch.Start();
            var rehydratedFlight = await eventStore.GetByIdAsync<Flight>(newFlight.Id);

            var timeToRehydrate = stopWatch.Elapsed;
            testOutput.WriteLine($"{noOfEvents} events loaded and applied in {timeToRehydrate}");

            const int expectedVersion = noOfEvents;

            newFlight.State.Version.Should().Be(expectedVersion);
            rehydratedFlight.State.Version.Should().Be(expectedVersion);
        }

        [Fact]
        public async Task AggregateStateIsCorrectAfterRehydration()
        {
            const string originalDestination = "Hong Kong";
            const string originalFlightNumber = "BA1987";
            const string newDestination = "Barra";
            const string newFlightNumber = "BA1986";

            var newFlight = new Flight();

            //Act
            newFlight.UpdateDestination(originalDestination);
            newFlight.UpdateFlightNumber(originalFlightNumber);

            await eventStore.SaveAsync(newFlight);

            var firstRehydration = await eventStore.GetByIdAsync<Flight>(newFlight.Id);

            firstRehydration.UpdateDestination(newDestination);
            firstRehydration.UpdateFlightNumber(newFlightNumber);

            await eventStore.SaveAsync(firstRehydration);

            var secondRehydration = await eventStore.GetByIdAsync<Flight>(newFlight.Id);

            //Assert
            secondRehydration.State.Destination.Should().Be(newDestination);
            secondRehydration.State.FlightNumber.Should().Be(newFlightNumber);
        }

        [Fact]
        public void RaisingAnAggregateEventThatIsNotHandledThrowsCorrectException()
        {
            var flight = new Flight();

            Assert.Throws<NotImplementedException>(() => flight.AddBaggageToHold(3));
        }

        [Fact]
        public Task ThrowsCorrectExceptionWhenLoadingNonPersistedAggregate()
        {
            return Assert.ThrowsAsync<AggregateNotFoundException>(() => eventStore.GetByIdAsync<Flight>(Guid.NewGuid()));
        }

        [Fact]
        public async Task ThrowsCorrectExceptionWhenLoadingSoftDeletedAggregate()
        {
            var flight = new Flight();
            var streamName = StreamName(flight);

            //Act
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(23));
            await eventStore.SaveAsync(flight);

            //use the eventstore connection directly
            await eventStore.Connection.DeleteStreamAsync(streamName, ExpectedVersion.Any);

            await Assert.ThrowsAsync<AggregateNotFoundException>(() => eventStore.GetByIdAsync<Flight>(flight.Id));
        }

        [Fact]
        public async Task ThrowsCorrectExceptionWhenLoadingHardDeletedAggregate()
        {
            const bool hardDelete = true;
            var flight = new Flight();
            var streamName = StreamName(flight);

            //Act
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(23));
            await eventStore.SaveAsync(flight);
            await eventStore.Connection.DeleteStreamAsync(streamName, ExpectedVersion.Any, hardDelete);

            await Assert.ThrowsAsync<AggregateDeletedException>(() => eventStore.GetByIdAsync<Flight>(flight.Id));
        }

        [Fact]
        public async Task CorrectlyRetrievesSpecificVersion()
        {
            var flightToBePersisted = new Flight();

            //Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            await eventStore.SaveAsync(flightToBePersisted);

            var rehydratedFlight = await eventStore.GetByIdAsync<Flight>(flightToBePersisted.Id, 5);

            //Assert
            rehydratedFlight.State.Version.Should().Be(5);
        }

        [Fact]
        public async Task CanRepeatedlyPersistAggregate()
        {
            var flightToBePersisted = new Flight();

            //Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            await eventStore.SaveAsync(flightToBePersisted);

            for (var i = 11; i <= 20; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            await eventStore.SaveAsync(flightToBePersisted);
        }

        private static string StreamName(Aggregate flight)
        {
            var streamName = $"{typeof(Flight).Name}-{flight.Id}";
            return streamName;
        }
    }
}
