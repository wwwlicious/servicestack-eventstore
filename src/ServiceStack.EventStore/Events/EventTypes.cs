// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HelperClasses;

    /// <summary>
    /// Contains a dictionary of DTOs that can be transformed from EventStore's ResolvedEvent
    /// </summary>
    internal static class EventTypes
    {
        private static readonly Dictionary<string, Type> eventTypes = new Dictionary<string, Type>();

        public static IReadOnlyDictionary<string, Type> GetAllHandlers()
        {
            return eventTypes;
        }

        public static bool TryResolveMapping(string eventType, out Type type)
        {
            eventTypes.TryGetValue(eventType, out type);
            return type != null;
        }

        public static bool HasMappings()
        {
            return eventTypes.HasAny();
        }

        public static void ScanForEvents(IReadOnlyList<Assembly> assembliesWithEvents)
        {

            foreach (var assembly in assembliesWithEvents)
            {
                var types = Assembly.Load(assembly.GetName())
                                    .GetTypes()
                                    .Where(t => t.IsClass && t.IsVisible);

                foreach (var type in types)
                {
                    eventTypes.Add(type.Name, type);
                }
            }
        }
    }
}
