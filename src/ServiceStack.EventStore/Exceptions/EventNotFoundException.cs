// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Exceptions
{
    using System;

    [Serializable]
    public class EventNotFoundException: Exception
    {
        public readonly string StreamId;
        public readonly int Position;

        public EventNotFoundException(string streamId, int position)
        {
            StreamId = streamId;
            Position = position;
        }
    }
}