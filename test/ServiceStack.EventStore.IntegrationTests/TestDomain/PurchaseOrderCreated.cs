// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class PurchaseOrderCreated : IAggregateEvent
    {
        public Guid Id { get; }

        public PurchaseOrderCreated(Guid id)
        {
            Id = id;
        }
    }
}