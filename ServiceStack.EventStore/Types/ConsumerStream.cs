namespace ServiceStack.EventStore.Types
{
    public struct ConsumerStream
    {
        public ConsumerStream(SubscriptionType subscriptionType, string subscriptionGroup)
        {
            
            SubscriptionType = subscriptionType;
            SubscriptionGroup = subscriptionGroup;
        }

        public SubscriptionType SubscriptionType { get; }
        public string SubscriptionGroup { get;  }
    }
}
