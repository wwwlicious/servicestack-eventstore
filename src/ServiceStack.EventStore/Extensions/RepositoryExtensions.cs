namespace ServiceStack.EventStore.Extensions
{
    using System;
    using System.Collections.Generic;
    using Repository;
    using Types;
    using System.Threading.Tasks;

    public static class RepositoryExtensions
    {
        public static async Task<TAggregate> SaveAsync<TAggregate>(this TAggregate @this, IEventStoreRepository repo, Action<IDictionary<string, object>> headers = null) 
            where TAggregate : Aggregate
        {
            await repo.SaveAsync(@this, headers);
            return @this;
        }
    }
}
