using System.Collections.Generic;
using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Repository
{
    using Types;
    using System;
    using System.Threading.Tasks;


    public interface IEventStoreRepository
    {
        Task Save(Aggregate aggregate, Action<IDictionary<string, object>> updateHeaders = null);

        void Publish(Event @event);

        Task<TAggregate> GetById<TAggregate>(Guid id, int version) where TAggregate : Aggregate;

        Task<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : Aggregate;

        IEventStoreConnection Connection { get; }
    }
}