// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Repository
{
    public class SliceSize
    {
        /// <summary>The first event in a stream</summary>
        public const int Min = 1;
        /// <summary>The last event in the stream.</summary>
        public const int Max = 4096;
    }
}