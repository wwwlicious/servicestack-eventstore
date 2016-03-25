namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Dispatcher;
    using Repository;
    using Types;
    using Logging;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;

    public class CatchUpConsumer : IEventConsumer
    {
        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository eventStoreRepository;
        private readonly ILog log;
        private string streamName;
        private string subscriptionGroup;

        public CatchUpConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
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
                await Task.Run(() => connection.SubscribeToStreamFrom(streamName, StreamPosition.Start, true,
                             async (subscription, @event) => await EventAppeared(subscription, @event),
                             async (subscription) => await LiveProcessingStarted(subscription),
                             async (subscription, reason, exception) => await SubscriptionDropped(subscription, reason, exception)));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            log.Error($"Subscription to {streamName} dropped. Message: {ex.Message}");
            await ConnectToSubscription(streamName, subscriptionGroup);
        }

        private async Task LiveProcessingStarted(EventStoreCatchUpSubscription @event)
        {
            await Task.Run(() => log.Info($"Caught up on {@event.StreamId} at {DateTime.UtcNow}"));
        }

        private async Task EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event != null)
            {
                if (!dispatcher.Dispatch(resolvedEvent))
                {
                    await eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
                }
            }
        }
    }
}
