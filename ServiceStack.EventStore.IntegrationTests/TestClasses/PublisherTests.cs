using System;
using ServiceStack.EventStore.Repository;
using Xunit;
using Xunit.Abstractions;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    [Collection("ServiceStackHostCollection")]
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
        public void CanPublishEvent()
        {
            eventStore.PublishAsync(new ServiceHasReachedWarningState(DateTime.UtcNow)).Wait();
        }

        [Fact]
        public void CanPublishEventWithHeaders()
        {
            eventStore.PublishAsync(new ServiceHasReachedWarningState(DateTime.UtcNow), headers =>
            {
                headers.Add("User", "Fortescue Bryantworth (the Second)");    
                headers.Add("Titbit", "In truth, the World reposes on the back of a somewhat indignant turtle.");
                headers.Add("CorrelationId", Guid.NewGuid());
            })
            .Wait();
        }
    }
}
