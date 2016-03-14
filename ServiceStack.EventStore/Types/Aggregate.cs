namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Generic;

    public abstract class Aggregate
    {
        protected List<IDomainEvent> changes;

        protected Aggregate(Guid id, IState state)
        {
            Id = id;
            State = state;
            changes = new List<IDomainEvent>();
        }

        protected void Causes(IDomainEvent @event)
        {
            changes.Add(@event);
            ApplyEvent(@event);
        }

        public Guid Id { get; }

        public IState State { get; }

        public IReadOnlyList<IDomainEvent> Changes => changes.AsReadOnly();

        public abstract void ApplyEvent(IDomainEvent @event);
    }

    public class Aggregate<TState> : Aggregate where TState : IState
    {
        protected Aggregate(Guid id, TState state): base(id, state)
        {
            State = state;
        }
        public new TState State { get; }

        public override void ApplyEvent(IDomainEvent @event)
        {
            State.Apply(@event);
        }
    }
}
