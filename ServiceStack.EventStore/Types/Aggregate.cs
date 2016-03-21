namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an event-sourced aggregate
    /// </summary>
    public abstract class Aggregate
    {
        protected List<IAggregateEvent> changes;

        protected Aggregate(Guid id, IState state)
        {
            Id = id;
            State = state;
            changes = new List<IAggregateEvent>();
        }

        protected void Causes(IAggregateEvent @event)
        {
            changes.Add(@event);
            ApplyEvent(@event);
        }

        /// <summary>
        /// The unique identifier of the aggregate
        /// </summary>
        public Guid Id { get; }

        public IState State { get; }

        public IReadOnlyList<IAggregateEvent> Changes => changes.AsReadOnly();

        /// <summary>
        /// Clears committed domain events. Used after persisting an aggregate.
        /// </summary>
        public void ClearCommittedEvents()
        {
            changes.Clear();
        }

        public abstract void ApplyEvent(IAggregateEvent @event);
    }

    public class Aggregate<TState> : Aggregate where TState : IState
    {
        protected Aggregate(Guid id): base(id, Activator.CreateInstance<TState>())
        {
            State = (TState) base.State;
        }

        public new TState State { get; }

        public override void ApplyEvent(IAggregateEvent @event)
        {
            State.Apply(@event);
        }
    }
}
