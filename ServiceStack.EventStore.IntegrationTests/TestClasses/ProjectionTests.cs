using ServiceStack.EventStore.Projections;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System.Threading.Tasks;
    using Repository;
    using Xunit;
    using Xunit.Abstractions;
    using FluentAssertions;
    using TestDomain;

    [Collection("ServiceStackHostCollection")]
    [Trait("Category", "Integration")]
    public class ProjectionTests
    {
        private readonly IEventStoreRepository eventStore;
        private readonly ITestOutputHelper testOutput;

        public ProjectionTests(ServiceStackHostFixture fixture, ITestOutputHelper output)
        {
            eventStore = fixture.AppHost.Container.Resolve<IEventStoreRepository>();
            testOutput = output;
        }

        [Fact]

        public async Task CanPopulateViewModel()
        {
            //var builder = new ReadModelBuilder<PurchaseOrderViewModel>();

        }

    }
}
