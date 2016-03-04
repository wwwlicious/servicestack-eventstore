namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public abstract class EventSourcedAggregate
    {
        public EventSourcedAggregate(Guid id)
        {
            Id = id;
            Changes = new List<IDomainEvent>();
        }

        public Guid Id { get; }

        public int Version { get; set; }

        public IList<IDomainEvent> Changes { get; }

        public abstract void ApplyEvent(IDomainEvent @event);

        public abstract ICollection GetUncommittedEvents();

        //void ClearUncommittedEvents();

        public abstract IMemento GetSnapshot();
    }
}
