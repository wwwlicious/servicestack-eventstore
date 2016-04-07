// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Health
{
    using System;

    // todo: will expose the state of health of the plugin
    [Route("/healthcheck")]
    public class EventStoreHealthService: Service
    {
        public void Any(EventStoreCheck check)
        {
            //logic
            throw new NotImplementedException();
        }
    }
}
