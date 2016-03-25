namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Dispatcher;
    using Repository;
    using Types;
    using Logging;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Subscriptions;

    public class CatchUpConsumer : IEventConsumer
    {
        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository eventStoreRepository;
        private readonly ILog log;

        public CatchUpConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
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
                await Task.Run(() => connection.SubscribeToStreamFrom(streamId, StreamPosition.Start, true,
                             async (subscription, @event) => await EventAppeared(subscription, @event),
                             async (subscription) => await LiveProcessingStarted(subscription),
                             async (subscription, reason, exception) => await SubscriptionDropped(subscription, reason, exception)));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason dropReason, Exception exception)
        {
            var streamId = subscription.StreamId;

            await SubscriptionPolicy.Handle(streamId, dropReason, exception, async () => await ConnectToSubscription(streamId, string.Empty));
        }

        private async Task LiveProcessingStarted(EventStoreCatchUpSubscription @event)
        {
            await Task.Run(() => log.Info($"Caught up on {@event.StreamId} at {DateTime.UtcNow}"));
        }

        private async Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event != null)
            {
                var dispatched = await dispatcher.Dispatch(resolvedEvent);

                if (!dispatched)
                {
                    await eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
                }
            }
        }
    }
}
