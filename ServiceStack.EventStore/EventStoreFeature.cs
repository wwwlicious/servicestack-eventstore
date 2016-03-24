namespace ServiceStack.EventStore
{
    using ServiceStack;
    using System;
    using System.Collections.Generic;
    using Funq;
    using Types;
    using Consumers;
    using Logging;
    using ConnectionManagement;
    using Dispatcher;
    using Resilience;
    using Repository;
    using global::EventStore.ClientAPI;

    public class EventStoreFeature: IPlugin
    {
        private readonly EventStoreSettings settings;
        private readonly EventTypes.EventTypes eventTypes;
        private Container container;
        private readonly ILog log;
        private readonly ConnectionManagement.ConnectionSettings connectionSettings;
        private ConnectionMonitor connectionMonitor;

        private readonly Dictionary<SubscriptionType, Type> consumers = new Dictionary<SubscriptionType, Type>()
        {
           [SubscriptionType.Persistent] = typeof(PersistentConsumer), 
           [SubscriptionType.CatchUp] = typeof(CatchUpConsumer), 
           [SubscriptionType.Volatile] = typeof(VolatileConsumer)
        };

        public EventStoreFeature(EventStoreSettings settings, ConnectionManagement.ConnectionSettings connectionSettings)
        {
            this.settings = settings;
            eventTypes = new EventTypes.EventTypes();
            log = LogManager.GetLogger(GetType());
            this.connectionSettings = connectionSettings;
        }

        public async void Register(IAppHost appHost)
        {
            var connection = EventStoreConnection.Create(connectionSettings.GetConnectionString());

            await connection.ConnectAsync().ConfigureAwait(false); //no need for the initial synchronisation context 
                                                                   //to be reused when executing the rest of the method

            new ConnectionMonitor(connection, connectionSettings.MonitorSettings).AddHandlers();

            container = appHost.GetContainer();

            RegisterTypesForIoc(connection);

            appHost.GetPlugin<MetadataFeature>()?.AddPluginLink($"http://{connectionSettings.GetHttpEndpoint()}/", "EventStore");

            try
            {
                foreach (var stream in settings.Streams)
                {
                    var consumer = (IEventConsumer)container.TryResolve(consumers[stream.Value.SubscriptionType]);
                    await consumer.ConnectToSubscription(stream.Key, stream.Value.SubscriptionGroup);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private void RegisterTypesForIoc(IEventStoreConnection connection)
        {
            container.RegisterAutoWired<PersistentConsumer>();
            container.RegisterAutoWired<CatchUpConsumer>();
            container.RegisterAutoWired<VolatileConsumer>();
            container.RegisterAutoWiredAs<CircuitBreaker, ICircuitBreaker>();
            container.RegisterAutoWiredAs<EventStoreRepository, IEventStoreRepository>();
            container.RegisterAutoWiredAs<EventDispatcher, IEventDispatcher>().ReusedWithin(ReuseScope.Default);
            container.Register(c => connection).ReusedWithin(ReuseScope.Default);
            container.Register(c => eventTypes).ReusedWithin(ReuseScope.Default);
        }
    }
}
