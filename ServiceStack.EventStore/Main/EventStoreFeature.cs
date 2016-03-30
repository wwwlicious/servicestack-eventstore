using ServiceStack.EventStore.EventTypeManagement;

namespace ServiceStack.EventStore.Main
{
    using System;
    using System.Collections.Generic;
    using global::EventStore.ClientAPI;
    using Funq;
    using ConnectionManagement;
    using Consumers;
    using Dispatcher;
    using Repository;
    using Subscriptions;
    using Logging;
    using Redis;
    using EventTypes = EventTypes;

    using ConnectionSettings = ConnectionManagement.ConnectionSettings;

    public class EventStoreFeature: IPlugin
    {
        private readonly SubscriptionSettings settings;
        private Container container;
        private readonly ILog log;
        private readonly ConnectionSettings connectionSettings;

        private string redisConnectionString = "127.0.0.1:6379"; //todo read this in

        private readonly Dictionary<string, Type> consumers = new Dictionary<string, Type>()
        {
            {"PersistentSubscription", typeof(PersistentConsumer)},
            {"CatchUpSubscription", typeof(CatchUpConsumer)},
            {"VolatileSubscription", typeof(VolatileConsumer)},
            {"ReadModelSubscription", typeof(ReadModelConsumer)}
        };

        public EventStoreFeature(SubscriptionSettings settings, ConnectionSettings connectionSettings)
        {
            this.settings = settings;
            log = LogManager.GetLogger(GetType());
            this.connectionSettings = connectionSettings;
            EventTypes.ScanForAggregateEvents();
            EventTypes.ScanForServiceEvents();
        }

        public async void Register(IAppHost appHost)
        {
            var connection = EventStoreConnection.Create(connectionSettings.GetConnectionString());

            await connection.ConnectAsync().ConfigureAwait(false); //no need for the initial synchronisation context 
                                                                   //to be reused when executing the rest of the method

            new ConnectionMonitor(connection, connectionSettings.MonitorSettings)
                    .AddHandlers();

            container = appHost.GetContainer();

            RegisterTypesForIoc(connection);

            appHost.GetPlugin<MetadataFeature>()?
                   .AddPluginLink($"http://{connectionSettings.GetHttpEndpoint()}/", "EventStore");

            try
            {
                foreach (var subscription in settings.Subscriptions)
                {
                    var consumer = (StreamConsumer)container.TryResolve(consumers[subscription.GetType().Name]);
                    await consumer.ConnectToSubscription(subscription);
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
            container.RegisterAutoWired<ReadModelConsumer>();
            container.RegisterAutoWiredAs<EventStoreRepository, IEventStoreRepository>();
            container.RegisterAutoWiredAs<EventDispatcher, IEventDispatcher>().ReusedWithin(ReuseScope.Default);
            container.Register<IRedisClientsManager>(c => new RedisManagerPool(redisConnectionString));
            container.Register(c => connection).ReusedWithin(ReuseScope.Container);
        }
    }
}
