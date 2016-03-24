namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Polly;
    using Logging;
    using Dispatcher;
    using Types;
    using Repository;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;

    public class PersistentConsumer: IEventConsumer
    {
        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository eventStoreRepository;
        private Policy policy;
        private readonly ILog log;
        private string streamName;
        private string subscriptionGroup;

        public PersistentConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.eventStoreRepository = eventStoreRepository;
            log = LogManager.GetLogger(GetType());
        }

        public async Task ConnectToSubscription(string streamName, string subscriptionGroup)
        {
            this.streamName = streamName;
            this.subscriptionGroup = subscriptionGroup;

            try
            {
                await Task.Run(() => connection.ConnectToPersistentSubscription(streamName, subscriptionGroup,
                     async (@base, @event) => await EventAppeared(@base, @event), 
                     async (@base, reason, exception) => await SubscriptionDropped(@base, reason, exception)));
            }
            catch (AggregateException aggregate)
            {
                foreach (var exception in aggregate.Flatten().InnerExceptions)
                {
                    log.Error(exception);
                }
            }
        }

        private async Task SubscriptionDropped(EventStorePersistentSubscriptionBase subscriptionBase, SubscriptionDropReason dropReason, Exception e)
        {
            await ConnectToSubscription(streamName, subscriptionGroup);
        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase @base, ResolvedEvent resolvedEvent)
        {
            if (!dispatcher.Dispatch(resolvedEvent))
                {
                 await eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
                }
            }
        }
    }
