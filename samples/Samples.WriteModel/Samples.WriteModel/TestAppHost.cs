namespace Samples.WriteModel
{
    using System.Reflection;
    using Funq;
    using ServiceStack;
    using ServiceStack.EventStore.ConnectionManagement;
    using ServiceStack.EventStore.Main;
    using ServiceStack.Logging;

    internal class TestAppHost : AppHostBase
    {
        public TestAppHost(string serviceName, params Assembly[] assembliesWithServices) 
            : base(serviceName, assembliesWithServices) { }

        public override void Configure(Container container)
        {
            var connection = new EventStoreConnectionSettings()
                            .UserName("admin")
                            .Password("changeit")
                            .TcpEndpoint("localhost:1113")
                            .HttpEndpoint("localhost:2113");


            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new EventStoreFeature(connection, typeof(TestAppHost).Assembly));
            Plugins.Add(new MetadataFeature());
        }
    }
}
