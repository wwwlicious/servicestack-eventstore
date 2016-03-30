using ServiceStack.EventStore.StateManagement;

namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Generic;
    using HelperClasses;
    using State = StateManagement.State;

    /// <summary>
    /// Represents an event-sourced aggregate
    /// Original source: https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/Aggregate.cs
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

    public abstract class Aggregate<TState> : Aggregate where TState : State, new()
    {
        protected Aggregate(Guid id): base(id, New<TState>.Instance())
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
