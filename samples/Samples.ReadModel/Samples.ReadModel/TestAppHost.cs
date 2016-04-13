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
            //when using a ReadModelSubscription we do not pass in the stream name
            //since it subscribes to all streams (the $all projection in event store)
            var subscriptionSettings = new SubscriptionSettings()
                                                    .SubscribeToStreams(s => s.Add(new ReadModelSubscription()
                                                        .WithStorage(new ReadModelStorage(StorageType.Redis, "localhost:6379"))));

            //these three settings are the minimum configuration required to 
            //set up a connection to EventStore
            var connectionSettings = new EventStoreConnectionSettings()
                                        .UserName("admin")
                                        .Password("changeit")
                                        .TcpEndpoint("localhost:1113");

            //we instantiate the plugin using the connection and subscription data as well as a 
            //parameter array of the assemblies where the plugin can find the CLR event types
            Plugins.Add(new EventStoreFeature(connectionSettings, subscriptionSettings, typeof(TestAppHost).Assembly));
        }
    }
}
