// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using System.Threading.Tasks;
    using Repository;
    using Xunit;
    using Xunit.Abstractions;

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
