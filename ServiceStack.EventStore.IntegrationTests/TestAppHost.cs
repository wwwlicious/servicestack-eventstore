using System.Net;
using Funq;
using ServiceStack.EventStore.ConnectionManagement;
using ServiceStack.EventStore.IntegrationTests.TestClasses;
using ServiceStack.EventStore.Subscriptions;
using ServiceStack.EventStore.Types;
using ServiceStack.Host;
using ServiceStack.Logging;
using ServiceStack.Web;

namespace ServiceStack.EventStore.IntegrationTests
{
    public class TestAppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public TestAppHost() : base("EventStoreListener", typeof(TestAppHost).Assembly)
        {

        }

        //public override IServiceRunner<TRequest> CreateServiceRunner<TRequest>(ActionContext actionContext)
        //{
        //    //Cached per Service Action
        //    return new EventStoreServiceRunner<TRequest>(this, actionContext);
        //}

        public override void Configure(Container container)
        {
            var settings = new SubscriptionSettings()
                                .MaxSubscriptionRetries(10)
                                .SubscribeToStreams(streams =>
                                {
                                    streams.Add(new VolatileSubscription("alien-landings"));
                                });

            var connection = new ConnectionSettings()
                                .UserName("admin")
                                .Password("changeit")
                                .HttpAddress("localhost:1113");

            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new MetadataFeature());
            Plugins.Add(new EventStoreFeature(settings, connection));
        }
    }
}
