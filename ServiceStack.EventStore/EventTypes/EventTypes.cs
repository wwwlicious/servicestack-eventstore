namespace ServiceStack.EventStore.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HelperClasses;

    /// <summary>
    /// Contains a dictionary of mappings between event types and their handlers i.e. IHandleEvent
    /// </summary>
    public class EventTypes
    {
        private readonly Dictionary<string, Type> eventTypes = new Dictionary<string, Type>();

        public EventTypes()
        {
            ScanForMappings();
        }

        public IDictionary<string, Type> GetAllHandlers()
        {
            return eventTypes;
        }

        public bool TryResolveMapping(string eventType, out Type type)
        {
            eventTypes.TryGetValue(eventType, out type);
            return type != null;
        }

        public bool HasMappings()
        {
            return eventTypes.HasAny();
        }

        private void ScanForMappings()
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
    }
}
