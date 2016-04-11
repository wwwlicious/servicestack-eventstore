// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ServiceStack.EventStore.Repository;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    public class WeeGreenManService: Service
    {
        private readonly IEventStoreRepository _repo;

        public WeeGreenManService(IEventStoreRepository repo)
        {
            _repo = repo;
        }

        public void Any(WeeGreenMenLanded @event)
        {
            Console.WriteLine("here");
        }

        public async Task DoSomething()
        {
            await _repo.PublishAsync(new SomethingHappened(), "targetstream", headers =>
            {

            });
        }
    }

    public class SomethingHappened
    {
    }
}
