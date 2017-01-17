// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::EventStore.ClientAPI;
    using Repository;
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
        public void SameEventIsWrittenOnlyOnce()
        {
            var streamName = $"IdempotentTestStream-{Guid.NewGuid()}";
            var timeStamp = DateTime.UtcNow;
            var evt = new ServiceHasReachedWarningState(timeStamp);

            eventStore.PublishAsync(evt, streamName);
            eventStore.PublishAsync(evt, streamName);

            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = eventStore
                                    .Connection
                                    .ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false)
                                    .Result;

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            }
            while (!currentSlice.IsEndOfStream);

            streamEvents.Count.Should().Be(1);
        }
    }
}