// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.IntegrationTests
{
    using FluentAssertions;
    using Funq;
    using global::EventStore.ClientAPI.Embedded;
    using Logging;
    using Main;
    using Projections;
    using Subscriptions;

    public class TestAppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public TestAppHost() : base("EventStoreListener", typeof(TestAppHost).Assembly) { }

        public override void Configure(Container container)
        {

            LogManager.LogFactory = new ConsoleLogFactory();

            var nodeBuilder = EmbeddedVNodeBuilder.AsSingleNode()
                           .OnDefaultEndpoints()
                           .RunInMemory();
            var node = nodeBuilder.Build();
            node.StartAndWaitUntilReady().Wait();


            Plugins.Add(new MetadataFeature());
            Plugins.Add(new EventStoreFeature(node, typeof(TestAppHost).Assembly));
        }
    }
}
