namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Factory for StateMutator objects
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
            var mutatorType = typeof(StateMutator<>).MakeGenericType(stateType);
            var mutator = Activator.CreateInstance(mutatorType);

            return mutator as IStateMutator;
        }
    }

    /// <summary>
    /// Mediator class responsible for mutating aggregate State classes.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    internal class StateMutator<TState> : IStateMutator where TState : IState
    {
        readonly Dictionary<string, Action<TState, object>> eventMutators;

        public StateMutator()
        {
            eventMutators = ReflectionUtils.GetStateEventMutators<TState>();
        }

        /// <summary>
        /// Mutates the State class of an Aggregate in response to a DomainEvent.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="event"></param>
        public void Mutate(IState state, IDomainEvent @event)
        {
            Action<TState, object> eventMutator;
            eventMutators.TryGetValue(@event.GetType().Name, out eventMutator);

            if (eventMutator == null)
                throw new NotImplementedException($"No handler has been implemented for event {@event.GetType().Name}");

            eventMutator((TState)state, @event);
        }
    }
}
