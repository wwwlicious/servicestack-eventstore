using System;
using EventStore.ClientAPI;
using Polly;
using ServiceStack.EventStore.Dispatcher;
using ServiceStack.EventStore.Repository;
using ServiceStack.EventStore.Types;
using ServiceStack.Logging;

namespace ServiceStack.EventStore.Consumers
{
    public class VolatileConsumer: IEventConsumer
    {

        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository eventStoreRepository;
        private Policy policy;
        private readonly ILog log;
        private string streamName;
        private string subscriptionGroup;

        public VolatileConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.eventStoreRepository = eventStoreRepository;
            log = LogManager.GetLogger(GetType());
        }

        public async void ConnectToSubscription(string streamName, string subscriptionGroup)
        {
            this.streamName = streamName;
            this.subscriptionGroup = subscriptionGroup;

            try
            {
                await connection.SubscribeToStreamAsync(streamName, false, EventAppeared, SubscriptionDropped);
            }
            catch (AggregateException aggregate)
            {
                foreach (var exception in aggregate.InnerExceptions)
                {
                    log.Error(exception);
                }
            }
        }

        private void SubscriptionDropped(EventStoreSubscription eventStoreSubscription, SubscriptionDropReason subscriptionDropReason, Exception arg3)
        {
            ConnectToSubscription(streamName, subscriptionGroup);
        }

        private void EventAppeared(EventStoreSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            if (!dispatcher.Dispatch(resolvedEvent))
            {
                eventStoreRepository.Publish(new InvalidMessage(resolvedEvent.OriginalEvent));
            }
        }
    }
}
