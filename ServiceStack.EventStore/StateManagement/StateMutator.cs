namespace ServiceStack.EventStore.StateManagement
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using HelperClasses;
    using Types;

    /// <summary>
    /// Factory for StateMutator objects
    /// Original Source: https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/StateMutator.cs
    /// </summary>
    internal static class StateMutator
    {
        static readonly ConcurrentDictionary<Type, IStateMutator> mutators;

        static StateMutator()
        {
            mutators = new ConcurrentDictionary<Type, IStateMutator>();
        }

        /// <summary>
        /// Specifies the State object to be mutated.
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        public static IStateMutator For(Type stateType)
        {
            return mutators.GetOrAdd(stateType, t => CreateMutator(stateType));
        }

        private static IStateMutator CreateMutator(Type stateType)
        {
            return New.CreateGenericInstance(typeof(StateMutator<>), stateType) as IStateMutator;
        }
    }

    /// <summary>
    /// Mediator class responsible for mutating aggregate State classes.
    /// </summary>
    /// <typeparam name="TState">An object representing the state of an aggregate.</typeparam>
    internal class StateMutator<TState> : IStateMutator where TState : IState, new()
    {
        readonly Dictionary<string, Action<TState, object>> eventMutators;

        public StateMutator()
        {
            eventMutators = ReflectionUtils.GetStateEventMutators<TState>();
        }

        /// <summary>
        /// Mutates the State class of an Aggregate in response to a DomainEvent.
        /// </summary>
        /// <param name="state">The current state of the aggregate.</param>
        /// <param name="event">The event to be applied to the state.</param>
        public void Mutate(IState state, IAggregateEvent @event)
        {
            Action<TState, object> eventMutator;
            eventMutators.TryGetValue(@event.GetType().Name, out eventMutator);

            if (eventMutator == null)
                throw new NotImplementedException($"No handler has been implemented for event {@event.GetType().Name}");

            eventMutator((TState)state, @event);
        }
    }
}
