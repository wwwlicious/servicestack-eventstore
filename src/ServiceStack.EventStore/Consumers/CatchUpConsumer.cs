// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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
            catch (AggregateException exception)
            {
                exception.InnerExceptions.Each(e => log.Error(e));
            }
        }

        private async Task SubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason dropReason, Exception exception)
        {
            var subscriptionDropped = new DroppedSubscription(this.subscription, exception.Message, dropReason);
            await HandleDroppedSubscription(subscriptionDropped);
        }

        private Task LiveProcessingStarted(EventStoreCatchUpSubscription @event) => 
            Task.Run(() => log.Info($"Caught up on {@event.StreamId} at {DateTime.UtcNow}"));

        private async Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent) => 
            await Dispatch(resolvedEvent);
    }
}
