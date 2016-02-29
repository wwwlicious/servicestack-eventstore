namespace ServiceStack.EventStore
{
    using Types;
    using FluentValidation;

    public class EventStoreSettings
    {
        private string deadLetterChannel = "dead-letters";
        private string subscriptionGroup = "consumer-group";
        private string invalidMessageChannel = "invalid-messages";
        private string consumerStream;
        private string publisherStream;
        private SubscriptionType subscriptionType = Types.SubscriptionType.Persistent;
        private StorageType storageType = StorageType.InMemory;

        private Validator validator = new Validator();

        private class Validator : AbstractValidator<EventStoreSettings>
        {
            public Validator()
            {
                
            }
        }

        public string GetConsumerStream()
        {
            return consumerStream;
        }

        public string GetPublisherStream()
        {
            return publisherStream;
        }

        public EventStoreSettings PublisherStream(string streamName)
        {
            publisherStream = streamName;
            return this;
        }

        public string GetSubscriptionGroup()
        {
            return subscriptionGroup;
        }

        public SubscriptionType GetSubscriptionType()
        {
            return subscriptionType;
        }

        public StorageType GetGuaranteedDeliveryType()
        {
            return storageType;
        }

        public EventStoreSettings StoreAndForward(StorageType type)
        {
            storageType = type;
            return this;
        }

        public EventStoreSettings ConsumerStream(string streamName)
        {
            consumerStream = streamName;
            return this;
        }

        public EventStoreSettings SubscriptionGroup(string @group)
        {
            subscriptionGroup = @group;
            return this;
        }

        public EventStoreSettings InvalidMessageChannel(string channel)
        {
            invalidMessageChannel = channel;
            return this;
        }

        public EventStoreSettings DeadLetterChannel(string channel)
        {
            deadLetterChannel = channel;
            return this;
        }

        public EventStoreSettings SubscriptionType(SubscriptionType type)
        {
            subscriptionType = type;
            return this;
        }
    }
}