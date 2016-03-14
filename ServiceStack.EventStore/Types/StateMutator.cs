namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Helpers;

    internal static class StateMutator
    {
        static readonly ConcurrentDictionary<Type, IStateMutator> mutators;

        static StateMutator()
        {
            mutators = new ConcurrentDictionary<Type, IStateMutator>();
        }

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

    internal class StateMutator<TState> : IStateMutator where TState : IState
    {
        readonly Dictionary<string, Action<TState, object>> eventMutators;

        public StateMutator()
        {
            eventMutators = ReflectionUtils.GetStateEventMutators<TState>();
        }

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
