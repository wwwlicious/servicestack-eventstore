// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Exceptions
{
    using System;

    /// <summary>
    /// An exception thrown when a requested (aggregate) stream has been deleted.
    /// </summary>
    public class AggregateDeletedException : Exception
    {
        public readonly Guid Id;
        public readonly Type Type;

        public AggregateDeletedException(Guid id, Type type) 
            : base($"Aggregate '{id}' (type {type.Name}) was deleted.")
        {
            Id = id;
            Type = type;
        }
    }
}