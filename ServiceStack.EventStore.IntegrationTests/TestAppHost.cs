namespace ServiceStack.EventStore.IntegrationTests
{
    using Funq;
    using ConnectionManagement;
    using Resilience;
    using Subscriptions;
    using Logging;
    using System;
    using FluentAssertions;
    using Main;
    using Projections;

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
            var settings = new EventStoreFeatureSettings()
                                .SubscribeToStreams(s =>
                                {
                                    s.Add(new VolatileSubscription("alien-landings")
                                            .SetRetryPolicy(5.Retries(), e => TimeSpan.FromSeconds(Math.Pow(2, e))));
                                    s.Add(new PersistentSubscription("", "")
                                            .SetRetryPolicy(new [] {1.Seconds(), 3.Seconds()}));
                                    s.Add(new ReadModelSubscription());
                                })
                                .WithReadModel(new ReadModelStorage(StorageType.Redis, "127.0.0.1:6379"));

        var connection = new EventStoreConnectionSettings()
                                .UserName("admin")
                                .Password("changeit")
                                .TcpEndpoint("localhost:1113")
                                .HttpEndpoint("localhost:2113");

            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new MetadataFeature());
            Plugins.Add(new EventStoreFeature(settings, connection));
        }
    }


}
