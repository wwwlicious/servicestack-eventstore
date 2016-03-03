using System.Runtime.Serialization;

namespace ServiceStack.EventStore.Types
{
    public abstract class AggregateEvent<TId> where TId: struct 
    {
        [IgnoreDataMember]
        public string AggregateStream { get;  }

        public TId AggregateId { get; }

        protected AggregateEvent(string aggregateStream) : this(aggregateStream, default(TId))
        {
            AggregateStream = aggregateStream;
        }

        protected AggregateEvent(string aggregateStream, TId aggregateId)
        {
            AggregateStream = aggregateStream;
            AggregateId = aggregateId;
        }
    }
}
