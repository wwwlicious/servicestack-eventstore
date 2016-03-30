using System;
using FluentAssertions;
using ServiceStack.EventStore.Main;

namespace ServiceStack.EventStore.IntegrationTests
{
    using Funq;
    using ConnectionManagement;
    using HelperClasses;
    using Resilience;
    using Subscriptions;
    using Logging;

    public class TestAppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public TestAppHost() : base("EventStoreListener", typeof(TestAppHost).Assembly)
        {

        }

        public override void Configure(Container container)
        {
            var settings = new SubscriptionSettings()
                                .SubscribeToStreams(streams =>
                                {
                                    streams.Add(new VolatileSubscription("alien-landings")
                                            .SetRetryPolicy(5.Retries(), e => TimeSpan.FromSeconds(Math.Pow(2, e))));
                                    streams.Add(new PersistentSubscription("", "")
                                            .SetRetryPolicy(new [] {1.Seconds(), 3.Seconds()}));
                                });

            var connection = new ConnectionSettings()
                                .UserName("admin")
                                .Password("changeit")
                                .TCPEndpoint("localhost:1113").HttpAddress("localhost:2113");

            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new MetadataFeature());
            Plugins.Add(new EventStoreFeature(settings, connection));
        }
    }
}
