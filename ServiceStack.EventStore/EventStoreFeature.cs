using EventStore.ClientAPI;

namespace ServiceStack.EventStore
{
    using ServiceStack;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Funq;
    using Types;
    using Consumers;
    using Logging;
    using ConnectionManagement;
    using Dispatcher;
    using Publisher;

    public class EventStoreFeature: IPlugin
    {
        private readonly EventStoreSettings settings;
        private readonly HandlerMappings mappings;
        private Container container;
        private readonly ILog log;
        private readonly ConnectionBuilder builder;
        private ConnectionMonitor _connectionMonitor;

        private readonly Dictionary<SubscriptionType, Type> consumers = new Dictionary<SubscriptionType, Type>()
        {
           [SubscriptionType.Persistent] = typeof(PersistentConsumer), 
           [SubscriptionType.CatchUp] = typeof(CatchUpConsumer), 
           [SubscriptionType.Volatile] = typeof(VolatileConsumer)
        };

        public EventStoreFeature(EventStoreSettings settings, ConnectionBuilder builder)
        {
            this.settings = settings;
            mappings = new HandlerMappings();
            log = LogManager.GetLogger(GetType());
            this.builder = builder;
        }

        public EventStoreFeature(EventStoreSettings settings, HandlerMappings mappings, ConnectionBuilder builder) : this(settings, builder)
        {
            this.mappings = mappings;
        }

        public async void Register(IAppHost appHost)
        {
            var connection = EventStoreConnection.Create(builder.GetConnectionString());

            await connection.ConnectAsync();

            new ConnectionMonitor(connection)
                    .Configure();

            container = appHost.GetContainer();

            if (!mappings.HasMappings())
            {
                ScanForMappings();
            }

            RegisterHandlerMappingsForIoc();
            RegisterTypesForIoc(connection);

            try
            {
                var subscriptionType = settings.GetSubscriptionType();
                var consumer = (IEventConsumer)container.TryResolve(consumers[subscriptionType]);
                consumer.ConnectToSubscription(settings.GetConsumerStream(), settings.GetSubscriptionGroup());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void RegisterTypesForIoc(IEventStoreConnection connection)
        {
            container.RegisterAutoWired<PersistentConsumer>();
            container.RegisterAutoWired<CatchUpConsumer>();
            container.RegisterAutoWired<VolatileConsumer>();
            container.RegisterAutoWiredAs<EventPublisher, IPublisher>();
            container.RegisterAutoWiredAs<EventDispatcher, IEventDispatcher>();
            container.Register(c => connection).ReusedWithin(ReuseScope.Default);
            container.Register(c => mappings).ReusedWithin(ReuseScope.Default);
        }

        private void RegisterHandlerMappingsForIoc()
        {
            foreach (var mapping in mappings.GetAllHandlers())
            {
                foreach (var handle in mapping.Value)
                {
                    container.RegisterAutoWiredType(handle);
                }
            }
        }

        private void ScanForMappings()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(path, "*.dll");
            var assemblies = new List<Assembly>(files.Length);

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var matchingHandlers = GetMatchingHandlersForAssembly(assembly);

                foreach (var handlerType in matchingHandlers)
                {
                    mappings.Add(handlerType);
                }

                if (matchingHandlers.Any())
                {
                    assemblies.Add(assembly);
                }
            }
        }

        private static Type[] GetMatchingHandlersForAssembly(Assembly assembly)
        {
            return assembly.GetExportedTypes()
                        .Where(x => x.IsOrHasGenericInterfaceTypeOf(typeof(IHandle<>)))
                        .Select(t => t).ToArray();
        }
    }
}
