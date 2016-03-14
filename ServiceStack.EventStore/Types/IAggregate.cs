using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.EventStore.Types
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }

        IState State { get; }
        IEnumerable<IDomainEvent> Changes { get; }
    }

    public interface IAggregate<out TState> : IAggregate where TState : IState
    {
        new TState State { get; }
    }
}
