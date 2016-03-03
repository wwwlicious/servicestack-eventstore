using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Polly;
    using Logging;
    using Dispatcher;
    using Types;
    using Publisher;

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
            catch (AggregateException aggregate)
            {
                foreach (var exception in aggregate.InnerExceptions)
                {
                    log.Error(exception);
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
