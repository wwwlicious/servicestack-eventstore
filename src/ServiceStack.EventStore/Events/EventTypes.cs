// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.Events
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using HelperClasses;
    using Extensions;

    /// <summary>
    /// Contains a dictionary of DTOs that can be transformed from EventStore's ResolvedEvent
    /// </summary>
    internal static class EventTypes
    {
        private static readonly Dictionary<string, Type> eventTypes = new Dictionary<string, Type>();

        public static IReadOnlyDictionary<string, Type> GetAllHandlers() => eventTypes;

        public static bool HasMappings() => eventTypes.HasAny();

        public static bool TryResolveMapping(string eventType, out Type type)
        {
            eventTypes.TryGetValue(eventType, out type);
            return type != null;
        }

        public static void ScanForEvents(IReadOnlyList<Assembly> assembliesWithEvents) => 
                assembliesWithEvents
                    .GetVisibleClasses()
                    .Each(t => eventTypes.Add(t.Name, t));
    }
}
