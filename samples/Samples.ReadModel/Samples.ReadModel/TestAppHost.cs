namespace Samples.ReadModel
{
    using System.Reflection;
    using Funq;
    using ServiceStack;
    using ServiceStack.EventStore.Projections;
    using ServiceStack.EventStore.Subscriptions;
    using ServiceStack.EventStore.ConnectionManagement;
    using ServiceStack.EventStore.Main;

    internal class TestAppHost : AppHostBase
    {
        public TestAppHost(string serviceName, params Assembly[] assembliesWithServices) 
            : base(serviceName, assembliesWithServices) { }

        public override void Configure(Container container)
        {
            var subscriptionSettings = new SubscriptionSettings()
                                                    .SubscribeToStreams(s => s.Add(new ReadModelSubscription()
                                                        .WithStorage(new ReadModelStorage(StorageType.Redis, "localhost:6379"))));

            var connectionSettings = new EventStoreConnectionSettings()
                                        .UserName("admin")
                                        .Password("changeit")
                                        .TcpEndpoint("localhost:1113");

            Plugins.Add(new EventStoreFeature(connectionSettings, subscriptionSettings, typeof(TestAppHost).Assembly));
        }
    }
}
