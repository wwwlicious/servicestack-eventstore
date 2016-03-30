namespace ServiceStack.EventStore.EventTypeManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using HelperClasses;
    using Types;

    /// <summary>
    /// Contains a dictionary of DTOs that can be transformed from EventStore's ResolvedEvent
    /// </summary>
    internal static class EventTypes
    {
        private static readonly Dictionary<string, Type> eventTypes = new Dictionary<string, Type>();

        internal static IReadOnlyDictionary<string, Type> GetAllHandlers()
        {
            return eventTypes;
        }

        internal static bool TryResolveMapping(string eventType, out Type type)
        {
            eventTypes.TryGetValue(eventType, out type);
            return type != null;
        }

        internal static bool HasMappings()
        {
            return eventTypes.HasAny();
        }

        internal static void ScanForServiceEvents()
        {
            var methods = HostContext.Metadata.ServiceTypes
                                                    .SelectMany(t => t.GetMethods()
                                                    .Where(m => m.IsPublic && m.Name == "Any"));

            var parameters = methods.SelectMany(m => m.GetParameters().Where(p => p.ParameterType.IsClass));

            foreach (var p in parameters)
            {
                eventTypes.Add(p.ParameterType.Name, p.ParameterType);
            }
        }

        internal static void ScanForAggregateEvents()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(path, "*.dll");

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var types = assembly.GetTypes()
                                .Where(t => t.IsClass && t.HasInterface(typeof(IAggregateEvent)));

                foreach (var type in types)
                {
                    eventTypes.Add(type.Name, type);
                }
            }
        }
    }
}
