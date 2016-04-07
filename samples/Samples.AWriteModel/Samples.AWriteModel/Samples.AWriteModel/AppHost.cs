using System;
using Funq;
using ServiceStack;
using Samples.AWriteModel.ServiceInterface;
using ServiceStack.EventStore.ConnectionManagement;
using ServiceStack.EventStore.Main;
using ServiceStack.EventStore.Projections;
using ServiceStack.EventStore.Resilience;
using ServiceStack.EventStore.Subscriptions;
using ServiceStack.Logging;

namespace Samples.AWriteModel
{
    public class AppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("Samples.AWriteModel", typeof(PurchaseOrderService).Assembly)
        {

        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            var connection = new EventStoreConnectionSettings()
                                    .UserName("admin")
                                    .Password("changeit")
                                    .TcpEndpoint("localhost:1113")
                                    .HttpEndpoint("localhost:2113");

            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new MetadataFeature());
            Plugins.Add(new EventStoreFeature(connection));
        }
    }
}
