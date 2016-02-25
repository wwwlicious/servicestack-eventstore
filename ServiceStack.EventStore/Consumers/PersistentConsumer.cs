using EventStore.ClientAPI;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Polly;
    using Logging;
    using Dispatcher;

    public class PersistentConsumer: IEventConsumer
    {
        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IPublisher publisher;
        private Policy policy;
        private readonly ILog log;
        private string streamName;
        private string subscriptionGroup;

        public PersistentConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IPublisher publisher)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.publisher = publisher;
            log = LogManager.GetLogger(GetType());
        }

        public void ConnectToSubscription(string streamName, string subscriptionGroup)
        {
            this.streamName = streamName;
            this.subscriptionGroup = subscriptionGroup;

            try
            {
                connection.ConnectToPersistentSubscription(streamName, subscriptionGroup, EventAppeared, SubscriptionDropped);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException.GetType() != typeof(InvalidOperationException)
                    && ex.InnerException?.Message != $"Subscription group {subscriptionGroup} on stream {streamName} already exists")
                {
                    throw;
                }
            }
        }

        private void SubscriptionDropped(EventStorePersistentSubscriptionBase subscriptionBase, SubscriptionDropReason dropReason, Exception e)
        {
            ConnectToSubscription(streamName, subscriptionGroup);
        }

        private void EventAppeared(EventStorePersistentSubscriptionBase @base, ResolvedEvent resolvedEvent)
        {
            if (!resolvedEvent.Event.IsJson || !dispatcher.Dispatch(resolvedEvent))
                {
                 publisher.Publish(new InvalidMessage(resolvedEvent.OriginalEvent));
                }
            }
        }
    }
