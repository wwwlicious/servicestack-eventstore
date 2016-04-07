// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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