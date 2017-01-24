// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentAssertions;
    using global::EventStore.ClientAPI;
    using Repository;
    using TestDomain;
    using Xunit;
    using Xunit.Abstractions;
    using ReadDirection = Repository.ReadDirection;

    [Collection("ServiceStackHostCollection")]
    [Trait("Category", "Integration")]
    public class ReaderTests
    {
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper testOutput;

        public ReaderTests(ServiceStackHostFixture fixture, ITestOutputHelper output)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            testOutput = output;
        }

        [Fact]
        public async Task CanReadSingleEventFromCorrectPositionInStream()
        {
            var streamName = $"ReaderTestStream-{Guid.NewGuid()}";

            await eventStore.PublishAsync(new LastPolled { Time = DateTime.UtcNow, Sequence = "Start" }, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = DateTime.UtcNow, Sequence = "Middle" }, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = DateTime.UtcNow, Sequence = "End" }, streamName).ConfigureAwait(false);

            var firstEvent = await eventStore.ReadEventAsync<LastPolled>(streamName, WhereInStream.Start).ConfigureAwait(false);
            firstEvent.Sequence.Should().Be("Start");

            var lastEvent = await eventStore.ReadEventAsync<LastPolled>(streamName, WhereInStream.End).ConfigureAwait(false);
            lastEvent.Sequence.Should().Be("End");

            var middleEvent = await eventStore.ReadEventAsync<LastPolled>(streamName, 1).ConfigureAwait(false);
            middleEvent.Sequence.Should().Be("Middle");
        }

        [Fact]
        public async Task ReadSingleEventThrowsEventNotFoundException()
        {
            var streamName = $"ReaderTestStream-{Guid.NewGuid()}";

            //try to read from the stream when no events have been published to it yet
            await Assert.ThrowsAsync<EventNotFoundException>(async () => 
                    await eventStore.ReadEventAsync<LastPolled>(streamName, WhereInStream.Start).ConfigureAwait(false));
        }

        [Fact]
        public async Task CanReadStreamForwardsAsStronglyTypedObjects()
        {
            var streamName = $"ReaderTestStream-{Guid.NewGuid()}";

            var firstDate = DateTime.UtcNow.AddDays(-3);
            var secondDate = DateTime.UtcNow.AddDays(-2);
            var thirdDate = DateTime.UtcNow.AddDays(-1);
            var fourthDate = DateTime.UtcNow.AddDays(0);

            await eventStore.PublishAsync(new LastPolled { Time = firstDate}, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = secondDate}, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = thirdDate }, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = fourthDate}, streamName).ConfigureAwait(false);

            var eventSlice = await eventStore.ReadSliceFromStreamAsync<LastPolled>(streamName, ReadDirection.Forwards, StreamPosition.Start, SliceSize.Max).ConfigureAwait(false);
            var events = eventSlice.ToArray();

            events.Should().HaveCount(4);
            events.Should().BeOfType<LastPolled[]>();
            events[0].Time.Should().BeSameDateAs(firstDate);
            events[1].Time.Should().BeSameDateAs(secondDate);
            events[2].Time.Should().BeSameDateAs(thirdDate);
            events[3].Time.Should().BeSameDateAs(fourthDate);
        }

        [Fact]
        public async Task CanReadEventBackwardFromStreamAsStronglyTypedObject()
        {
            var streamName = $"ReaderTestStream-{Guid.NewGuid()}";

            var firstDate = DateTime.UtcNow.AddDays(-3);
            var secondDate = DateTime.UtcNow.AddDays(-2);
            var thirdDate = DateTime.UtcNow.AddDays(-1);
            var fourthDate = DateTime.UtcNow.AddDays(0);

            await eventStore.PublishAsync(new LastPolled { Time = firstDate }, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = secondDate }, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = thirdDate }, streamName).ConfigureAwait(false);
            await eventStore.PublishAsync(new LastPolled { Time = fourthDate }, streamName).ConfigureAwait(false);

            var eventSlice = await eventStore.ReadSliceFromStreamAsync<LastPolled>(streamName, ReadDirection.Backwards, StreamPosition.End, SliceSize.Max).ConfigureAwait(false);
            var events = eventSlice.ToArray();

            events.Should().HaveCount(4);
            events.Should().BeOfType<LastPolled[]>();
            events[0].Time.Should().BeSameDateAs(fourthDate);
            events[1].Time.Should().BeSameDateAs(thirdDate);
            events[2].Time.Should().BeSameDateAs(secondDate);
            events[3].Time.Should().BeSameDateAs(firstDate);
        }
    }
}