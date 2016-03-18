using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Polly;
    using Logging;
    using Dispatcher;
    using Types;
    using Repository;

    public class PersistentConsumer: IEventConsumer
    {
        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository _eventStoreRepository;
        private Policy policy;
        private readonly ILog log;
        private string streamName;
        private string subscriptionGroup;

        public PersistentConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository _eventStoreRepository)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this._eventStoreRepository = _eventStoreRepository;
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
            if (!dispatcher.Dispatch(resolvedEvent))
                {
                 _eventStoreRepository.Publish(new InvalidMessage(resolvedEvent.OriginalEvent));
                }
            }
        }
    }
