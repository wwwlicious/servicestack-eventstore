using Xunit.Sdk;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using FluentAssertions;
    using Repository;
    using Xunit;
    using Xunit.Abstractions;
    using System.Threading.Tasks;
    using Funq;

    [Collection("ServiceStackHostCollection")]
    [Trait("Category", "Integration")]
    public class EventHandlerTests 
    {
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper testOutput;
        private bool eventFired;

        public EventHandlerTests(ITestOutputHelper testOutput, ServiceStackHostFixture fixture)
        {
            this.testOutput = testOutput;
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            //fixture.AppHost.Container.RegisterAutoWired<VolatileConsumer>();
            fixture.AppHost.Container.Register(c => this).ReusedWithin(ReuseScope.Default);
        }

        // todo: how best to test handling events and streams?
        public async Task ConsumesEvent()
        {
            var weeGreenMenLanded = new WeeGreenMenLanded(14);

            await eventStore.PublishAsync(weeGreenMenLanded);
            await Task.Delay(10000);

            eventFired.Should().BeTrue();
        }

        public void Handle(WeeGreenMenLanded @event)
        {
            eventFired = true;
        }
    }
}
