namespace ServiceStack.EventStore
{
    using Types;
    using System;
    using System.Collections.Generic;

    public class EventStoreSettings
    {
        private string deadLetterChannel = "deadletters";
        private string subscriptionGroup = "consumer-group";
        private string invalidMessageChannel = "invalidmessages";

        private IDictionary<string, ConsumerStream> streams { get; } = new Dictionary<string, ConsumerStream>();

        public string GetSubscriptionGroup()
        {
            return subscriptionGroup;
        }

        public IReadOnlyDictionary<string, ConsumerStream> Streams => streams as IReadOnlyDictionary<string, ConsumerStream>;

        public EventStoreSettings SubscribeToStreams(Action<IDictionary<string, ConsumerStream>> updateStreams = null)
        {
            updateStreams?.Invoke(streams);
            return this;
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
    }
}