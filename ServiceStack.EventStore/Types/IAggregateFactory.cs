using System;

namespace ServiceStack.EventStore.Types
{
    public interface IAggregateFactory<out TAggregate> where TAggregate : IAggregate
    {
        TAggregate CreateNew(Guid id);
        TAggregate CreateFromEvents(Guid id, IAggregateEvent[] events);
        TAggregate CreateFromState(Guid id, IState state);
    }
}
