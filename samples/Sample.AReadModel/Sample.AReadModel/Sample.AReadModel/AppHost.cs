using Funq;
using ServiceStack;
using Sample.AReadModel.ServiceInterface;
using ServiceStack.Configuration;
using ServiceStack.EventStore.ConnectionManagement;
using ServiceStack.EventStore.Main;
using ServiceStack.EventStore.Projections;
using ServiceStack.EventStore.Subscriptions;

namespace Sample.AReadModel
{
    public class AppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("Sample.AReadModel", typeof(PurchaseOrderService).Assembly)
        {

        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            var featureSettings = new EventStoreFeatureSettings()
                                        .SubscribeToStreams(s => s.Add(new ReadModelSubscription()))
                                        .WithReadModel(new ReadModelStorage(StorageType.Redis, "localhost:6379"));

            var connectionSettings = new EventStoreConnectionSettings()
                                        .UserName("admin")
                                        .Password("changeit")
                                        .TcpEndpoint("localhost:1113");

            Plugins.Add(new EventStoreFeature(featureSettings, connectionSettings));
        }
    }
}
