// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.Main
{
    using System;
    using System.Linq;
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
    using System.Reflection;
    using Configuration;
    using global::EventStore.ClientAPI.Embedded;
    using global::EventStore.Core;

    public class EventStoreFeature: IPlugin
    {
        private readonly SubscriptionSettings subscriptionSettings;
        private Container container;
        private readonly ILog log;
        private readonly Assembly[] assembliesWithEvents;

        private readonly Dictionary<string, Type> consumers = new Dictionary<string, Type>
        {
            {"PersistentSubscription", typeof(PersistentConsumer)},
            {"CatchUpSubscription", typeof(CatchUpConsumer)},
            {"VolatileSubscription", typeof(VolatileConsumer)},
            {"ReadModelSubscription", typeof(ReadModelConsumer)}
        };

        private IAppSettings appSettings;
        private ClusterVNode clusterNode;

        public EventStoreFeature(params Assembly[] assembliesWithEvents) : 
            this(new SubscriptionSettings(), assembliesWithEvents) { }

        public EventStoreFeature(SubscriptionSettings subscriptionSettings, params Assembly[] assembliesWithEvents)
        {
            this.assembliesWithEvents = assembliesWithEvents ?? new[] { Assembly.GetExecutingAssembly() };
            this.subscriptionSettings = subscriptionSettings;
            EventTypes.ScanForEvents(assembliesWithEvents);
            log = LogManager.GetLogger(GetType());
        }

        public EventStoreFeature(ClusterVNode clusterNode, SubscriptionSettings subscriptionSettings, params Assembly[] assembliesWithEvents)
            : this(subscriptionSettings, assembliesWithEvents)
        {
            this.clusterNode = clusterNode;
        }

        public async void Register(IAppHost appHost)
        {
            appSettings = appHost.AppSettings ?? new AppSettings();

            var connectionSettings = GetConnectionSettings();

            var connection = clusterNode != null 
                                ? EventStoreConnection.Create(connectionSettings.GetConnectionString())
                                : EmbeddedEventStoreConnection.Create(clusterNode);

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
                foreach (var subscription in subscriptionSettings.Subscriptions)
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

        private EventStoreConnectionSettings GetConnectionSettings()
        {
            var connectionSettings = new EventStoreConnectionSettings();

            connectionSettings.TcpEndpoint(appSettings.GetString("ServiceStack.Plugins.EventStore.TcpEndPoint"));
            connectionSettings.HttpEndpoint(appSettings.GetString("ServiceStack.Plugins.EventStore.HttpEndPoint"));
            connectionSettings.UserName(appSettings.GetString("ServiceStack.Plugins.EventStore.UserName"));
            connectionSettings.Password(appSettings.GetString("ServiceStack.Plugins.EventStore.Password"));

            return connectionSettings;
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
                subscriptionSettings.Subscriptions
                    .FirstOrDefault(s => s.GetType() == typeof (ReadModelSubscription))
                    .ConvertTo<ReadModelSubscription>();

            var readModel = readModelSubscription?.Storage();

            if (readModel != null)
                readModelDelegates[readModel.StorageType]?.Invoke(readModel.ConnectionString);
        }
    }
}
