// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using System.Linq;

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
    using Events;
    using Projections;

    public class EventStoreFeature: IPlugin
    {
        private readonly SubscriptionSettings featureSettings;
        private Container container;
        private readonly ILog log;
        private readonly EventStoreConnectionSettings connectionSettings;

        private readonly Dictionary<string, Type> consumers = new Dictionary<string, Type>
        {
            {"PersistentSubscription", typeof(PersistentConsumer)},
            {"CatchUpSubscription", typeof(CatchUpConsumer)},
            {"VolatileSubscription", typeof(VolatileConsumer)},
            {"ReadModelSubscription", typeof(ReadModelConsumer)}
        };

        public EventStoreFeature(EventStoreConnectionSettings connectionSettings): 
            this(connectionSettings, new SubscriptionSettings()) { }

        public EventStoreFeature(EventStoreConnectionSettings connectionSettings, SubscriptionSettings featureSettings)
        {
            this.featureSettings = featureSettings;
            this.connectionSettings = connectionSettings;
            EventTypes.ScanForAggregateEvents();
            EventTypes.ScanForServiceEvents();
            log = LogManager.GetLogger(GetType());
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
                foreach (var subscription in featureSettings.Subscriptions)
                {
                    var consumer = (StreamConsumer)container.TryResolve(consumers[subscription.GetType().Name]);
                    await consumer.ConnectToSubscription(subscription).ConfigureAwait(false);
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
            container.Register(c => connection).ReusedWithin(ReuseScope.Container);

            RegisterStorageTypes();
        }

        private void RegisterStorageTypes()
        {
            var readModelDelegates = new Dictionary<StorageType, Action<string>>
            {
                {StorageType.Redis, (cs) => container.Register<IRedisClientsManager>(c => new RedisManagerPool(cs))}
            };

            var readModelSubscription =
                featureSettings.Subscriptions
                    .FirstOrDefault(s => s.GetType() == typeof (ReadModelSubscription))
                    .ConvertTo<ReadModelSubscription>();

            var readModel = readModelSubscription?.Storage();

            if (readModel != null)
                readModelDelegates[readModel.StorageType]?.Invoke(readModel.ConnectionString);
        }
    }
}
