// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

namespace ServiceStack.EventStore.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using Projections;

    /// <summary>
    /// Settings required to manage subscriptions to EventStore.
    /// </summary>
    public class EventStoreFeatureSettings
    {
        private IList<Subscription> subscriptions { get; } = new List<Subscription>();
        private ReadModelStorage readModelStorage;

        public IReadOnlyList<Subscription> Subscriptions => subscriptions as IReadOnlyList<Subscription>;

        public EventStoreFeatureSettings SubscribeToStreams(Action<IList<Subscription>> updateSubscriptions = null)
        {
            updateSubscriptions?.Invoke(subscriptions);
            return this;
        }

        public ReadModelStorage ReadModel()
        {
            return readModelStorage;
        }

        public EventStoreFeatureSettings WithReadModel(ReadModelStorage readModelStorage)
        {
            this.readModelStorage = readModelStorage;
            return this;
        }
    }
}