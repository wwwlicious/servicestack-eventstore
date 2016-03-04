using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Funq;
using ServiceStack.EventStore.ConnectionManagement;
using ServiceStack.EventStore.Mappings;
using ServiceStack.Host.HttpListener;
using ServiceStack.Logging;

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
        public override void Configure(Container container)
        {
            var mappings = new HandlerMappings()
                                .UseAssemblyScanning();

            var settings = new EventStoreSettings();

            var connection = new ConnectionBuilder()
                                .UserName("admin")
                                .Password("changeit")
                                .Host("localhost:1113");

            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new EventStoreFeature(settings, mappings, connection));
        }


    }
}
