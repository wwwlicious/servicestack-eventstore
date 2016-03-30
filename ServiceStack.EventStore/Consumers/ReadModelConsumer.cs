namespace ServiceStack.EventStore.Consumers
{
    using System;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Dispatcher;
    using Repository;
    using Subscriptions;
    using Types;

    /// <summary>
    /// Represents the consumer of a catch-up subscription to all streams for the purpose of populating a read model
    /// </summary>
    internal class ReadModelConsumer : StreamConsumer
    {
        public ReadModelConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository) 
                : base(connection, dispatcher, eventStoreRepository) { }

        public override async Task ConnectToSubscription(Subscription subscription)
        {
            this.subscription = subscription;

            try
            {
                await Task.Run(() => connection.SubscribeToAllFrom(Position.Start, true, EventAppeared, LiveProcessingStarted, SubscriptionDropped));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async void SubscriptionDropped(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, SubscriptionDropReason subscriptionDropReason, Exception exception)
        {
            var subscriptionDropped = new DroppedSubscription(subscription, exception.Message, subscriptionDropReason);
            await HandleDroppedSubscription(subscriptionDropped);
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription eventStoreCatchUpSubscription)
        {
            log.Info("Read model now caught-up");
        }

        private async void EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
        {
            await Dispatch(resolvedEvent);
        }
    }
}
