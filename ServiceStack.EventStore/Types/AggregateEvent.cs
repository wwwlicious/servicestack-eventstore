namespace ServiceStack.EventStore.Types
{
    public abstract class AggregateEvent<TId> where TId: struct 
    {
        public string StreamName { get;  }

        public TId AggregateId { get; }

        protected AggregateEvent(string streamName, TId aggregateId)
        {
            StreamName = streamName;
            AggregateId = aggregateId;
        }
    }
}
