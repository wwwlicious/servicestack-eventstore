// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using Types;

    public class WeeGreenMenLanded
    {
        public Guid Id { get; }
        public int NoOfWeeGreenMen { get; }
        public DateTime TimeStamp { get; }

        public WeeGreenMenLanded(int noOfWeeGreenMen)
        {
            Id = Guid.NewGuid();
            NoOfWeeGreenMen = noOfWeeGreenMen;
            TimeStamp = DateTime.UtcNow;
        }
    }
}