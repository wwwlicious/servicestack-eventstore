using ServiceStack.EventStore.Subscriptions;

namespace ServiceStack.EventStore.Consumers
{
    using System;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Dispatcher;
    using Repository;
    using Types;
    using Logging;

    public class VolatileConsumer: IEventConsumer
    {

        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository eventStoreRepository;
        private readonly ILog log;

        public VolatileConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.eventStoreRepository = eventStoreRepository;
            log = LogManager.GetLogger(GetType());
        }

        public async Task ConnectToSubscription(string streamId, string subscriptionGroup)
        {
            try
            {
                await connection.SubscribeToStreamAsync(streamId, true,
                        async (subscription, @event) => await EventAppeared(subscription, @event),
                        async (subscription, reason, exception) => await SubscriptionDropped(subscription, reason, exception));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStoreSubscription subscription, SubscriptionDropReason subscriptionDropReason, Exception exception)
        {
            var streamId = subscription.StreamId;

            await SubscriptionPolicy.Handle(streamId, subscriptionDropReason, exception, async () => await ConnectToSubscription(streamId, string.Empty));
        }

        private async Task EventAppeared(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var dispatched = await dispatcher.Dispatch(resolvedEvent);

            if (!dispatched)
            {
                await eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
            }
        }
    }
}
