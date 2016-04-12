// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.Consumers
{
    using Resilience;
    using System.Threading.Tasks;
    using System;
    using global::EventStore.ClientAPI;
    using Dispatcher;
    using Repository;
    using Subscriptions;
    using Logging;
    using Types;

    /// <summary>
    /// The abstract base class which supports the different consumer types
    /// </summary>
    internal abstract class StreamConsumer
    {
        protected readonly IEventDispatcher dispatcher;
        protected readonly IEventStoreConnection connection;
        protected readonly IEventStoreRepository eventStoreRepository;
        protected readonly ILog log;
        protected Subscription subscription;

        protected StreamConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.eventStoreRepository = eventStoreRepository;
            log = LogManager.GetLogger(GetType());
        }

        //set the default RetryPolicy for each subscription - max 5 retries with exponential backoff
        protected RetryPolicy retryPolicy = new RetryPolicy(5.Retries(), retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public abstract Task ConnectToSubscription(Subscription subscription);

        protected async Task Dispatch(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event != null && !IsSystemEvent(resolvedEvent))
            {
                await dispatcher.Dispatch(resolvedEvent);
            }
        }

        protected async Task HandleDroppedSubscription(DroppedSubscription subscriptionDropped) => 
            await DroppedSubscriptionPolicy.Handle(subscriptionDropped, async () => await ConnectToSubscription(subscription));

        private static bool IsSystemEvent(ResolvedEvent resolvedEvent) => 
            resolvedEvent.OriginalEvent.EventType.StartsWith("$");
    }
}
