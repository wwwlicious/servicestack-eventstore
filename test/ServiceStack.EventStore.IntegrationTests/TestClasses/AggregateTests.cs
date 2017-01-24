// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Exceptions;
    using Extensions;
    using FluentAssertions;
    using global::EventStore.ClientAPI;
    using Repository;
    using TestDomain;
    using Types;
    using Xunit;
    using Xunit.Abstractions;

    // NOTE: These integration tests require EventStore to be running locally!
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
            // Act
            var flightToBePersisted = 
                    await new Flight()
                                .UpdateFlightNumber("CQH8821")
                                .SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(1))
                                .SaveAsync(eventStore)
                                .ConfigureAwait(false);

            var flightToBeRehydrated = await eventStore.GetByIdAsync<Flight>(flightToBePersisted.Id);

            // Assert
            flightToBeRehydrated.Id.Should().Be(flightToBePersisted.Id);
        }

        [Fact]
        public void AggregateVersionIsCorrectAfterAddingEvents()
        {
            var flight = new Flight();

            // Act
            flight.UpdateDestination("Malaga");

            // Assert
            flight.State.Version.Should().Be(2);

            // Act
            flight.UpdateFlightNumber("BA1987");

            // Assert
            flight.State.Version.Should().Be(3);
        }

        [Fact]
        public void AggregateStateIsCorrectAfterApplyingEvents()
        {
            // Arrange
            const string testDestination = "Malaga";
            const string testFlightNumber = "BA1987";

            //Act
            var flight = new Flight()
                                .UpdateDestination(testDestination)
                                .UpdateFlightNumber(testFlightNumber);

            // Assert
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
            var flightToBePersisted = 
                await new Flight()
                        .UpdateDestination("Grimthorpe")
                        .UpdateFlightNumber("XY6876")
                        .SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(134))
                        .SaveAsync(eventStore)
                        .ConfigureAwait(false);

            var correctVersion = flightToBePersisted.State.Version;
            var incorrectVersion = correctVersion.Add(1);

            await
                Assert.ThrowsAsync<AggregateVersionException>(
                    () => eventStore.GetByIdAsync<Flight>(flightToBePersisted.Id, incorrectVersion));
        }

        [Fact]
        public async Task CanAddHeadersToAggregateEvents()
        {
            var flight = 
                await new Flight()
                            .UpdateDestination("La Palma")
                            .UpdateFlightNumber("LT7987")
                            .SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(67))
                            .SaveAsync(eventStore, headers =>
                            {
                                headers.Add("User", "Lil'Joey Brown");
                                headers.Add("Other", "SomeImportantInfo");
                            })
                            .ConfigureAwait(false);

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

            // Act
            await newFlight
                    .UpdateDestination(originalDestination)
                    .UpdateFlightNumber(originalFlightNumber)
                    .SaveAsync(eventStore)
                    .ConfigureAwait(false);

            var firstRehydration = await eventStore.GetByIdAsync<Flight>(newFlight.Id).ConfigureAwait(false);

            await firstRehydration
                    .UpdateDestination(newDestination)
                    .UpdateFlightNumber(newFlightNumber)
                    .SaveAsync(eventStore)
                    .ConfigureAwait(false);

            var secondRehydration = await eventStore.GetByIdAsync<Flight>(newFlight.Id).ConfigureAwait(false);

            // Assert
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
            return
                Assert.ThrowsAsync<AggregateNotFoundException>(
                    () => eventStore.GetByIdAsync<Flight>(Guid.NewGuid()));
        }

        [Fact]
        public async Task ThrowsCorrectExceptionWhenLoadingSoftDeletedAggregate()
        {
            var flight = new Flight();
            var streamName = StreamName(flight);

            // Act
            await flight
                    .SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(23))
                    .SaveAsync(eventStore)
                    .ConfigureAwait(false);

            // use the eventstore connection directly
            await eventStore.DeleteStreamAsync(streamName, ExpectedVersion.Any);

            await
                Assert.ThrowsAsync<AggregateNotFoundException>(
                    () => eventStore.GetByIdAsync<Flight>(flight.Id));
        }

        [Fact]
        public async Task ThrowsCorrectExceptionWhenLoadingHardDeletedAggregate()
        {
            const bool hardDelete = true;
            var flight = new Flight();
            var streamName = StreamName(flight);

            // Act
            await flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(23))
                        .SaveAsync(eventStore)
                        .ConfigureAwait(false);

            await eventStore.DeleteStreamAsync(streamName, ExpectedVersion.Any, hardDelete)
                            .ConfigureAwait(false);

            await
                Assert.ThrowsAsync<AggregateDeletedException>(
                    () => eventStore.GetByIdAsync<Flight>(flight.Id));
        }

        [Fact]
        public async Task CorrectlyRetrievesSpecificVersion()
        {
            var flightToBePersisted = new Flight();

            // Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            await eventStore.SaveAsync(flightToBePersisted);

            var rehydratedFlight = await eventStore.GetByIdAsync<Flight>(flightToBePersisted.Id, 5);

            // Assert
            rehydratedFlight.State.Version.Should().Be(5);
        }

        [Fact]
        public async Task CanRepeatedlyPersistAggregate()
        {
            var flightToBePersisted = new Flight();

            // Act
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