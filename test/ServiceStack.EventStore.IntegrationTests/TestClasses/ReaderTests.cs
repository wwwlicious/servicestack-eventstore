// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentAssertions;
    using Repository;
    using TestDomain;
    using Xunit;
    using Xunit.Abstractions;

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

        [Fact(Skip = "Stream reading not yet implemented")]
        public async Task CanReadStreamForwards()
        {
            var streamName = $"ReaderTestStream-{Guid.NewGuid()}";
            var publishedEvent = new LastPolled {Time = DateTime.UtcNow};

            await eventStore.PublishAsync(publishedEvent, streamName).ConfigureAwait(false);
        }
    }
}