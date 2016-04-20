// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    using System;
    using System.Threading.Tasks;
    using Repository;

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

        public Task DoSomething()
        {
            return _repo.PublishAsync(new SomethingHappened(), "targetstream", headers =>
             {

             });
        }
    }

    public class SomethingHappened
    {
    }
}
