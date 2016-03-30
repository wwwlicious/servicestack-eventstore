namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Dispatcher;
    using Repository;
    using Types;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Subscriptions;

    /// <summary>
    /// Represents the consumer of a catch-up subscription to EventStore: http://docs.geteventstore.com/introduction/subscriptions/
    /// </summary>
    internal class CatchUpConsumer : StreamConsumer
    {
        public CatchUpConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository) 
            : base(connection, dispatcher, eventStoreRepository) { }

        public override async Task ConnectToSubscription(Subscription subscription)
        {
            this.subscription = subscription;
            try
            {
                await Task.Run(() => connection.SubscribeToStreamFrom(subscription.StreamId, StreamPosition.Start, true,
                             async (sub, @event) => await EventAppeared(sub, @event),
                             async (sub) => await LiveProcessingStarted(sub),
                             async (sub, reason, exception) => await SubscriptionDropped(sub, reason, exception)));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason dropReason, Exception exception)
        {
            var subscriptionDropped = new DroppedSubscription(this.subscription, exception.Message, dropReason);

            await HandleDroppedSubscription(subscriptionDropped);
        }

        private Task LiveProcessingStarted(EventStoreCatchUpSubscription @event)
        {
            return Task.Run(() => log.Info($"Caught up on {@event.StreamId} at {DateTime.UtcNow}"));
        }

        private async Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            await Dispatch(resolvedEvent);
        }

    }
}
