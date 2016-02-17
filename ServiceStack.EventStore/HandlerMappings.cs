namespace ServiceStack.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Types;
    /// <summary>
    /// Contains a dictionary of mappings between event types and their handlers i.e. IHandle
    /// </summary>
    public class HandlerMappings
    {
        private readonly Dictionary<Type, List<Type>> mappings = new Dictionary<Type, List<Type>>();

        public HandlerMappings WithHandler<TEventHandler>() where TEventHandler: class
        {
            var eventType =
                typeof(TEventHandler)
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>))
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
            var eventType =
              handler.GetInterfaces()
                  .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>))
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
    }
}
