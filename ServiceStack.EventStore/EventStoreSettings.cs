namespace ServiceStack.EventStore
{
    using Types;

    public class EventStoreSettings
    {
        private string deadLetterChannel = "deadletters";
        private string subscriptionGroup = "consumer-group";
        private string invalidMessageChannel = "invalidmessages";
        private SubscriptionType subscriptionType = Types.SubscriptionType.Persistent;
   
        public string GetSubscriptionGroup()
        {
            return subscriptionGroup;
        }

        public SubscriptionType GetSubscriptionType()
        {
            return subscriptionType;
        }

        public EventStoreSettings SubscriptionGroup(string @group)
        {
            subscriptionGroup = @group;
            return this;
        }

        public string GetInvalidMessageChannel()
        {
            return invalidMessageChannel;
        }

        public EventStoreSettings InvalidMessageChannel(string channel)
        {
            invalidMessageChannel = channel;
            return this;
        }

        public string GetDeadLetterChannel()
        {
            return deadLetterChannel;
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