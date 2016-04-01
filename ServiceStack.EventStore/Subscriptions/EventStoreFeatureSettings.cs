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