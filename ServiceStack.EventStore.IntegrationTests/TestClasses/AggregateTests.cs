﻿using System.Diagnostics;
using EventStore.ClientAPI;
using ServiceStack.EventStore.Extensions;
using Xunit.Abstractions;
using Xunit.Sdk;

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
        private readonly ITestOutputHelper testOutput;

        public AggregateTests(ServiceStackHostFixture fixture, ITestOutputHelper output)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            testOutput = output;
        }

        [Fact]
        public void AnAggregateCanBeSavedAndRehydrated()
        {
            var flightToBePersisted = new Flight();

            //Act
            flightToBePersisted.UpdateFlightNumber("CQH8821");
            flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(1));
            eventStore.Save(flightToBePersisted).Wait();
            var flightToBeRehydrated = eventStore.GetById<Flight>(flightToBePersisted.Id).Result;

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
        public void GetsEventsFromCorrectStreams()
        {
            var firstFlight = new Flight();
            var secondFlight = new Flight();

            const string firstFlightNumber = "BA7651";
            const string secondlightNumber = "BA7652";

            firstFlight.UpdateFlightNumber(firstFlightNumber);
            secondFlight.UpdateFlightNumber(secondlightNumber);

            testOutput.WriteLine($"First Flight Id: {firstFlight.Id}");
            testOutput.WriteLine($"Second Flight Id: {secondFlight.Id}");

            eventStore.Save(firstFlight).Wait();
            eventStore.Save(secondFlight).Wait();

            var firstSaved = eventStore.GetById<Flight>(firstFlight.Id).Result;
            var secondSaved = eventStore.GetById<Flight>(secondFlight.Id).Result;

            firstSaved.State.FlightNumber.Should().Be(firstFlightNumber);
            secondSaved.State.FlightNumber.Should().Be(secondlightNumber);
        }

        [Fact]
        public void ThrowsOnRequestingSpecificVersionHigherThanExists()
        {
            var flightToBePersisted = new Flight();
            flightToBePersisted.UpdateDestination("Grimthorpe");
            flightToBePersisted.UpdateFlightNumber("XY6876");
            flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(134));

            eventStore.Save(flightToBePersisted);

            var correctVersion = flightToBePersisted.State.Version;
            var incorrectVersion = correctVersion.Add(1);

            Assert.ThrowsAsync<AggregateVersionException>(() => eventStore.GetById<Flight>(flightToBePersisted.Id, incorrectVersion));
        }

        //todo: currently no way of reading additional headers (other than CLR type) from events
        [Fact]
        public void CanAddHeadersToAggregateEvents()
        {
            var flight = new Flight();
            flight.UpdateDestination("La Palma");
            flight.UpdateFlightNumber("LT7987");
            flight.SetEstimatedDepartureTime(DateTime.UtcNow.AddMinutes(67));
            eventStore.Save(flight, headers =>
            {
                headers.Add("User", "Lil'Joey Brown");
                headers.Add("Other", "SomeImportantInfo");
            }).Wait();

            eventStore.GetById<Flight>(flight.Id).Wait();
        }

        [Fact]
        public void CanSaveAndRehydrateLargeNumberOfEvents()
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

            eventStore.Save(newFlight).Wait();

            var timeToSave = stopWatch.Elapsed;
            testOutput.WriteLine($"{noOfEvents} events written in {timeToSave}");

            stopWatch.Start();
            var rehydratedFlight = eventStore.GetById<Flight>(newFlight.Id).Result;

            var timeToRehydrate = stopWatch.Elapsed;
            testOutput.WriteLine($"{noOfEvents} events loaded and applied in {timeToRehydrate}");

            var expectedVersion = noOfEvents;

            newFlight.State.Version.Should().Be(expectedVersion);
            rehydratedFlight.State.Version.Should().Be(expectedVersion);
        }

        [Fact]
        public void AggregateStateIsCorrectAfterRehydration()
        {
            const string originalDestination = "Hong Kong";
            const string originalFlightNumber = "BA1987";
            const string newDestination = "Barra";
            const string newFlightNumber = "BA1986";

            var newFlight = new Flight();

            //Act
            newFlight.UpdateDestination(originalDestination);
            newFlight.UpdateFlightNumber(originalFlightNumber);

            eventStore.Save(newFlight).Wait();

            var firstRehydration = eventStore.GetById<Flight>(newFlight.Id).Result;

            firstRehydration.UpdateDestination(newDestination);
            firstRehydration.UpdateFlightNumber(newFlightNumber);

            eventStore.Save(firstRehydration).Wait();

            var secondRehydration = eventStore.GetById<Flight>(newFlight.Id).Result;

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
        public void ThrowsCorrectExceptionWhenLoadingNonPersistedAggregate()
        {
            Assert.ThrowsAsync<AggregateNotFoundException>(() => eventStore.GetById<Flight>(Guid.NewGuid())).Wait();
        }

        [Fact]
        public void ThrowsCorrectExceptionWhenLoadingSoftDeletedAggregate()
        {
            var flight = new Flight();
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
            var flight = new Flight();
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
            var flightToBePersisted = new Flight();

            //Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            eventStore.Save(flightToBePersisted).Wait();

            var rehydratedFlight = eventStore.GetById<Flight>(flightToBePersisted.Id, 5).Result;

            //Assert
            rehydratedFlight.State.Version.Should().Be(5);
        }

        [Fact]
        public void CanRepeatedlyPersistAggregate()
        {
            var flightToBePersisted = new Flight();

            //Act
            for (var i = 1; i <= 10; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
                flightToBePersisted.SetEstimatedDepartureTime(DateTime.UtcNow.AddHours(i));
            }

            eventStore.Save(flightToBePersisted).Wait();

            for (var i = 11; i <= 20; i++)
            {
                flightToBePersisted.UpdateDestination($"Destination No: {i}");
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
