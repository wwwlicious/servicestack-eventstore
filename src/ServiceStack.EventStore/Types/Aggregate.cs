// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Generic;
    using HelperClasses;
    using StateManagement;

    /// <summary>
    /// Represents an event-sourced aggregate
    /// Original source: https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/Aggregate.cs
    /// </summary>
    public abstract class Aggregate
    {
        protected List<dynamic> changes;

        protected Aggregate(Guid id, IState state)
        {
            Id = id;
            State = state;
            changes = new List<dynamic>();
        }

        protected void Causes<T>(T @event)
        {
            changes.Add(@event);
            ApplyEvent(@event);
        }

        /// <summary>
        /// The unique identifier of the aggregate
        /// </summary>
        public Guid Id { get; }

        public IState State { get; }

        public IReadOnlyList<dynamic> Changes => changes.AsReadOnly();

        /// <summary>
        /// Clears committed domain events. Used after persisting an aggregate.
        /// </summary>
        public void ClearCommittedEvents()
        {
            changes.Clear();
        }

        public abstract void ApplyEvent(dynamic @event);
    }

    public abstract class Aggregate<TState> : Aggregate where TState : State, new()
    {
        protected Aggregate(Guid id): base(id, New<TState>.Instance())
        {
            State = (TState) base.State;
        }

        public new TState State { get; }

        public override void ApplyEvent(dynamic @event)
        {
            State.Apply(@event);
        }
    }
}
