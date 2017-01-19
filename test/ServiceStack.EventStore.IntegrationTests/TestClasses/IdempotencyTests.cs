// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::EventStore.ClientAPI;
    using Repository;
    using TestDomain;
    using Xunit;
    using Xunit.Abstractions;

    [Collection("ServiceStackHostCollection")]
    [Trait("Category", "Integration")]
    public class IdempotencyTests
    {
        private readonly IEventStoreRepository eventStore;

        private readonly ITestOutputHelper testOutput;

        public IdempotencyTests(ServiceStackHostFixture fixture, ITestOutputHelper output)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            testOutput = output;
        }

        [Fact]
        public async Task SameEventIsIdempotent()
        {
            var streamName = $"IdempotentTestStream-{Guid.NewGuid()}";
            var timeStamp = DateTime.UtcNow;
            var evt = new ServiceHasReachedWarningState(timeStamp);

            await eventStore.PublishAsync(evt, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(evt, streamName).ConfigureAwait(false);

            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;

            do
            {
                currentSlice = await eventStore
                                        .Connection
                                        .ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false)
                                        .ConfigureAwait(false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            }
            while (!currentSlice.IsEndOfStream);

            streamEvents.Count.Should().Be(1);

            testOutput.WriteLine($"1 event idempotently written to {streamName}");
        }

        [Fact]
        public async Task SimilarEventWithDifferentTimeStampIsNotIdempotent()
        {
            var product = new Product {ProductCode = "MN2400" };
            var customer = new Customer {CustomerCode = "0000010367" };
            var firstOccurrence = DateTime.UtcNow.Subtract(new TimeSpan(3, 0, 0));
            var secondOccurrence = DateTime.UtcNow.Subtract(new TimeSpan(2, 0, 0));
            var evt1 = new PriceChanged {Product = product, Customer = customer, TimeStamp = firstOccurrence};
            var evt2 = new PriceChanged {Product = product, Customer = customer, TimeStamp = secondOccurrence};
            var streamName = $"IdempotentTestStream-{Guid.NewGuid()}";

            await eventStore.PublishAsync(evt1, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(evt2, streamName).ConfigureAwait(false);

            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;

            do
            {
                currentSlice = await eventStore
                                    .Connection
                                    .ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false)
                                    .ConfigureAwait(false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            }
            while (!currentSlice.IsEndOfStream);

            streamEvents.Count.Should().Be(2);

            testOutput.WriteLine($"2 events succesfully written to {streamName}");
        }
    }
}