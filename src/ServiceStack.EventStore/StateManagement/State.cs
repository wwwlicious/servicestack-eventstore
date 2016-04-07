// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.StateManagement
{
    using Types;

    /// <summary>
    /// Holds the state of an aggregate in memory and will be used as the snapshot for that aggregate.
    /// Original source: https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/State.cs
    /// </summary>
    public abstract class State : IState
    {
        private readonly IStateMutator mutator;
        private const int InitialVersion = 0;

        protected State()
        {
            mutator = StateMutator.For(GetType());
            Version = InitialVersion;
        }

        public int Version { get; private set; }

        public void Apply(IAggregateEvent @event)
        {
            mutator.Mutate(this, @event);
            Version++;
        }
    }
}
