// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System;
    using Repository;
    using Xunit;
    using Xunit.Abstractions;
    using System.Threading.Tasks;

    [Collection("ServiceStackHostCollection")]
    [Trait("Category", "Integration")]
    public class PublisherTests
    {
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper testOutput;

        public PublisherTests(ServiceStackHostFixture fixture, ITestOutputHelper output)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            testOutput = output;
        }

        [Fact]
        public Task CanPublishEvent()
        {
            return eventStore.PublishAsync(new ServiceHasReachedWarningState(DateTime.UtcNow), "");
        }

        [Fact]
        public Task CanPublishEventWithHeaders()
        {
            return eventStore.PublishAsync(new ServiceHasReachedWarningState(DateTime.UtcNow), "stream", headers =>
             {
                 headers.Add("User", "Fortescue Bryantworth (the Second)");
                 headers.Add("Titbit", "In truth, the World reposes on the back of a somewhat indignant turtle.");
                 headers.Add("CorrelationId", Guid.NewGuid());
             });
        }
    }
}
