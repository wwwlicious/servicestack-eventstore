namespace ServiceStack.EventStore.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Types;

    /// <summary>
    /// Contains a dictionary of mappings between event types and their handlers i.e. IHandleEvent
    /// </summary>
    public class HandlerMappings
    {
        private readonly Dictionary<Type, List<Type>> mappings = new Dictionary<Type, List<Type>>();
        private bool useAssemblyScanning;

        public HandlerMappings WithHandler<TEventHandler>() where TEventHandler: class, IHandleEvent
        {
            var eventType =
                typeof(TEventHandler)
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>))
                    .SelectMany(i => i.GenericTypeArguments)
                    .FirstOrDefault();

            if (eventType != null)
            {
                if (mappings.ContainsKey(eventType))
                {
                    mappings[eventType].Add(typeof(TEventHandler));
                }
                else
                {
                    mappings.Add(eventType, new List<Type> { typeof(TEventHandler) });
                }
            }

            return this;
        }

        public void Add(Type handler)  
        {
            //Get a list of all event classes that are handled by methods that implement IHandleEvent<TEvent>
            var eventType =
              handler.GetInterfaces()
                  .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>))
                  .SelectMany(i => i.GenericTypeArguments)
                  .FirstOrDefault();

            if (eventType != null)
            {
                if (mappings.ContainsKey(eventType))
                {
                    mappings[eventType].Add(handler);
                }
                else
                {
                    mappings.Add(eventType, new List<Type> { handler });
                }
            }
        }

        public IDictionary<Type, List<Type>> GetAllHandlers()
        {
            if (useAssemblyScanning)
            {
                ScanForMappings();
            }
            return mappings;
        }

        public IList<Type> GetHandlersForEvent(Type eventType)
        {
            return mappings[eventType];
        }

        public bool TryResolveMapping(string eventType, out Type type)
        {
            type = mappings.FirstOrDefault(t => t.Key.Name.Contains(eventType)).Key;
            return type != null;
        }

        public bool HasMappings()
        {
             return mappings.Any();
        }

        public HandlerMappings UseAssemblyScanning()
        {
            useAssemblyScanning = true;
            return this;
        }

        private void ScanForMappings()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(path, "*.dll");
            var assemblies = new List<Assembly>(files.Length);

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var matchingHandlers = GetMatchingHandlersForAssembly(assembly);

                foreach (var handlerType in matchingHandlers)
                {
                    Add(handlerType);
                }

                if (matchingHandlers.Any())
                {
                    assemblies.Add(assembly);
                }
            }
        }

        private static Type[] GetMatchingHandlersForAssembly(Assembly assembly)
        {
            return assembly.GetExportedTypes()
                        .Where(x => x.IsOrHasGenericInterfaceTypeOf(typeof(IHandleEvent<>)))
                        .Select(t => t).ToArray();
        }
    }
}
