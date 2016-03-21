﻿using System.Net;
using EventStore.ClientAPI;

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
    using Mappings;
    using Repository;

    public class EventStoreFeature: IPlugin
    {
        private readonly EventStoreSettings settings;
        private readonly Mappings.EventTypes _eventTypes;
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
            _eventTypes = new Mappings.EventTypes();
            log = LogManager.GetLogger(GetType());
            this.builder = builder;
        }

        public async void Register(IAppHost appHost)
        {
            var connection = EventStoreConnection.Create(builder.GetConnectionString());

            await connection.ConnectAsync();

            new ConnectionMonitor(connection, builder.MonitorSettings)
                    .Configure();

            container = appHost.GetContainer();

            RegisterTypesForIoc(connection);

            appHost.GetPlugin<MetadataFeature>()?.AddPluginLink("http://127.0.0.1:2113/", "EventStore");

            try
            {
                foreach (var stream in settings.Streams)
                {
                    var consumer = (IEventConsumer)container.TryResolve(consumers[stream.Value.SubscriptionType]);      
                    consumer.ConnectToSubscription(stream.Key, stream.Value.SubscriptionGroup);
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
            container.RegisterAutoWiredAs<EventDispatcher, IEventDispatcher>();
            container.Register(c => connection).ReusedWithin(ReuseScope.Default);
            container.Register(c => _eventTypes).ReusedWithin(ReuseScope.Default);
        }
    }
}
