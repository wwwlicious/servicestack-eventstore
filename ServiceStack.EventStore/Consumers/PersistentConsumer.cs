namespace ServiceStack.EventStore.Consumers
{
    using System;
    using Logging;
    using Dispatcher;
    using Types;
    using Repository;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Subscriptions;
    using Resilience;

    public class PersistentConsumer: IEventConsumer
    {
        private readonly IEventDispatcher dispatcher;
        private readonly IEventStoreConnection connection;
        private readonly IEventStoreRepository eventStoreRepository;
        private readonly ILog log;
        private string streamId;
        private string subscriptionGroup;
        private RetryPolicy retryPolicy;

        public PersistentConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher, IEventStoreRepository eventStoreRepository)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.eventStoreRepository = eventStoreRepository;
            log = LogManager.GetLogger(GetType());
        }

        public async Task ConnectToSubscription(string streamId, string subscriptionGroup)
        {
            this.streamId = streamId;
            this.subscriptionGroup = subscriptionGroup;

            try
            {
                await Task.Run(() => connection.ConnectToPersistentSubscription(streamId, subscriptionGroup,
                     async (@base, @event) => await EventAppeared(@base, @event), 
                     async (@base, reason, exception) => await SubscriptionDropped(@base, reason, exception)));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        public void SetRetryPolicy(RetryPolicy policy)
        {
            retryPolicy = policy;
        }

        private async Task SubscriptionDropped(EventStorePersistentSubscriptionBase subscriptionBase, SubscriptionDropReason dropReason, Exception exception)
        {
            var subscriptionDropped = new StreamSubscriptionDropped(streamId, exception.Message, dropReason, retryPolicy);

            await SubscriptionPolicy.Handle(subscriptionDropped, async () => await ConnectToSubscription(streamId, subscriptionGroup));
        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase @base, ResolvedEvent resolvedEvent)
        {
            var dispatched = await dispatcher.Dispatch(resolvedEvent);

            if (!dispatched)
            {
                await eventStoreRepository.PublishAsync(new InvalidMessage(resolvedEvent.OriginalEvent));
            }
        }
    }
}
