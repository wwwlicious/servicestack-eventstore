namespace ServiceStack.EventStore.Consumers
{
    using System;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Dispatcher;
    using Repository;
    using Types;
    using Subscriptions;

    /// <summary>
    /// Represents a consumer to a volatile subscription: http://docs.geteventstore.com/introduction/subscriptions/
    /// </summary>
    internal class VolatileConsumer: StreamConsumer
    {
        public VolatileConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository) 
                : base(connection, dispatcher, eventStoreRepository) { }

        public override async Task ConnectToSubscription(Subscription subscription)
        {
            this.subscription = subscription;

            try
            {
                await connection.SubscribeToStreamAsync(subscription.StreamId, true,
                        async (sub, @event) => await EventAppeared(sub, @event),
                        async (sub, reason, exception) => await SubscriptionDropped(sub, reason, exception));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStoreSubscription subscription, SubscriptionDropReason dropReason, Exception exception)
        {
            var subscriptionDropped = new DroppedSubscription(this.subscription, exception.Message, dropReason);

            await HandleDroppedSubscription(subscriptionDropped);
        }

        private async Task EventAppeared(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            await Dispatch(resolvedEvent);
        }
    }
}
