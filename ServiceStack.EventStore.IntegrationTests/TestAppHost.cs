using Funq;
using ServiceStack.EventStore.ConnectionManagement;
using ServiceStack.EventStore.IntegrationTests.TestClasses;
using ServiceStack.EventStore.Mappings;
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
            var settings = new EventStoreSettings()
                                .SubscribeToStreams(streams =>
                                {
                                    streams.Add("alien-landings", new ConsumerStream(SubscriptionType.Volatile, "mygroup"));
                                });

            var connection = new ConnectionBuilder()
                                .UserName("admin")
                                .Password("changeit")
                                .Host("localhost:1113");

            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new MetadataFeature());
            Plugins.Add(new EventStoreFeature(settings, connection));
        }
    }
}
