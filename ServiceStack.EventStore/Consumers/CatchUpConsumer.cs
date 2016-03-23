using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Dispatcher;
    using Repository;
    using Types;
    using Logging;

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


        public void ConnectToSubscription(string streamName, string subscriptionGroup)
        {
            this.streamName = streamName;
            this.subscriptionGroup = subscriptionGroup;

            try
            {
                connection.SubscribeToStreamFrom(streamName, StreamPosition.Start, true, EventAppeared, LiveProcessingStarted, SubscriptionDropped);
            }
            catch (AggregateException aggregate)
            {
                foreach (var exception in aggregate.Flatten().InnerExceptions)
                {
                    log.Error(exception);
                }
            }
        }

        private void SubscriptionDropped(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, SubscriptionDropReason subscriptionDropReason, Exception arg3)
        {
            ConnectToSubscription(streamName, subscriptionGroup);
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription caughtUp)
        {
            Console.WriteLine("I have caught up");
        }

        private void EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event != null)
            {
                if (!dispatcher.Dispatch(resolvedEvent))
                {
                    eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
                }
            }
        }
    }
}
