// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class BaggageAdded : IAggregateEvent
    {
        public int NoOfBags { get; }

        public BaggageAdded(int noOfBags)
        {
            NoOfBags = noOfBags;
        }
    }
}