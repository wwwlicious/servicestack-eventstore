namespace ServiceStack.EventStore.Types
{
    using System;
    using System.Collections.Concurrent;
    using Helpers;

    public class AggregateFactory
    {
        static readonly ConcurrentDictionary<Type, object> factories;

        public static IAggregateFactory<TAggregate> For<TAggregate>() where TAggregate : IAggregate
        {
            var factory = factories.GetOrAdd(typeof(TAggregate), t => CreateFactory<TAggregate>());

            return factory as IAggregateFactory<TAggregate>;
        }

        private static IAggregateFactory<TAggregate> CreateFactory<TAggregate>() where TAggregate : IAggregate
        {
            var memberInfo = typeof(TAggregate).BaseType;
            if (memberInfo != null)
            {
                var stateType = memberInfo.GetGenericArguments()[0];

                var factoryType = typeof(AggregateFactory<,>).MakeGenericType(typeof(TAggregate), stateType);

                return Activator.CreateInstance(factoryType) as IAggregateFactory<TAggregate>;
            }
            return null;
        }
    }

    internal class AggregateFactory<TAggregate, TState> : IAggregateFactory<TAggregate> 
                    where TAggregate : IAggregate<TState> where TState : IState, new()
    {
        readonly Func<Guid, TState, TAggregate> createAggregate;

        public AggregateFactory()
        {
            createAggregate = ReflectionUtils.BuildCreateAggregateFromStateFunc<TAggregate, TState>();
        }

        public TAggregate CreateNew(Guid id)
        {
            var state = new TState();

            return createAggregate(id, state);
        }

        public TAggregate CreateFromEvents(Guid id, IAggregateEvent[] events)
        {
            var state = new TState();

            foreach (var @event in events)
            {
                state.Mutate(@event);
            }

            return createAggregate(id, state);
        }

        public TAggregate CreateFromState(Guid id, IState state)
        {
            return createAggregate(id, (TState)state);
        }
    }
}
