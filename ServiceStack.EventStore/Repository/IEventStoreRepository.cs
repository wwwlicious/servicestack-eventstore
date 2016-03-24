namespace ServiceStack.EventStore.Repository
{
    using Types;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using global::EventStore.ClientAPI;

    public interface IEventStoreRepository
    {
        Task SaveAsync(Aggregate aggregate, Action<IDictionary<string, object>> updateHeaders = null);

        Task PublishAsync(Event @event, Action<IDictionary<string, object>> updateHeaders = null);

        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version) where TAggregate : Aggregate;

        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate;

        IEventStoreConnection Connection { get; }
    }
}