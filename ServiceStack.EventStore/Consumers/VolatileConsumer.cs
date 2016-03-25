namespace ServiceStack.EventStore.Consumers
{
    using System;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Polly;
    using Dispatcher;
    using Repository;
    using Types;
    using Logging;

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

        public async Task ConnectToSubscription(string streamName, string subscriptionGroup)
        {
            this.streamName = streamName;
            this.subscriptionGroup = subscriptionGroup;

            try
            {
                await connection.SubscribeToStreamAsync(streamName, true,
                    async (subscription, @event) => await EventAppeared(subscription, @event),
                    async (subscription, reason, exception) => await SubscriptionDropped(subscription, reason, exception));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStoreSubscription eventStoreSubscription, SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            await ConnectToSubscription(streamName, subscriptionGroup);
        }

        private async Task EventAppeared(EventStoreSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            if (!dispatcher.Dispatch(resolvedEvent))
            {
                await eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
            }
        }
    }
}
