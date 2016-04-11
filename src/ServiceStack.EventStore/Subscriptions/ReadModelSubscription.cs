// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using ServiceStack.EventStore.Projections;
using ServiceStack.EventStore.Resilience;

namespace ServiceStack.EventStore.Subscriptions
{
    /// <summary>
    /// Represents a catch-up subscription to EventStore for populating a read model
    /// </summary>
    public class ReadModelSubscription : Subscription
    {
        private ReadModelStorage readModelStorage;

        public ReadModelSubscription() : base(string.Empty, string.Empty)
        {
        }

        public new ReadModelSubscription SetRetryPolicy(params TimeSpan[] durations)
        {
            return (ReadModelSubscription) base.SetRetryPolicy(durations);
        }

        public new ReadModelSubscription SetRetryPolicy(Retries maxNoOfRetries, Func<int, TimeSpan> provider)
        {
            return (ReadModelSubscription) base.SetRetryPolicy(maxNoOfRetries, provider);
        }

        public Subscription WithStorage(ReadModelStorage readModelStorage)
        {
            this.readModelStorage = readModelStorage;
            return this;
        }

        internal ReadModelStorage Storage()
        {
            return readModelStorage;
        }
    }
}
