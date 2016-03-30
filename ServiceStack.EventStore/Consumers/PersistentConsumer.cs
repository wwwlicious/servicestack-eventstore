namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Dispatcher;
    using Types;
    using Repository;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Subscriptions;

    /// <summary>
    /// Represents the consumer of a persistent subscription to EventStore: http://docs.geteventstore.com/introduction/subscriptions/
    /// This kind of consumer supports the competing consumer messaging pattern: http://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html
    /// </summary>
    internal class PersistentConsumer: StreamConsumer
    {
        public PersistentConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository) 
            : base(connection, dispatcher, eventStoreRepository) { }

        public override async Task ConnectToSubscription(Subscription subscription)
        {
            this.subscription = subscription;

            try
            {
                await Task.Run(() => connection.ConnectToPersistentSubscription(this.subscription.StreamId, this.subscription.SubscriptionGroup,
                     async (@base, @event) => await EventAppeared(@base, @event), 
                     async (@base, reason, exception) => await SubscriptionDropped(@base, reason, exception)));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async Task SubscriptionDropped(EventStorePersistentSubscriptionBase subscriptionBase, SubscriptionDropReason dropReason, Exception exception)
        {
            var subscriptionDropped = new DroppedSubscription(this.subscription, exception.Message, dropReason);

            await HandleDroppedSubscription(subscriptionDropped);
        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase @base, ResolvedEvent resolvedEvent)
        {
            await Dispatch(resolvedEvent);
        }
    }
}
