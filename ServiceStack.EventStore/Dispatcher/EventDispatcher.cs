using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Dispatcher
{
    using System;
    using Funq;
    using Text;
    using Logging;
    using Mappings;

    public class EventDispatcher : IEventDispatcher
    {
        // ReSharper disable once InconsistentNaming
        public Container container = ServiceStackHost.Instance.Container;
        private readonly HandlerMappings mappings;
        private ILog log;

        public EventDispatcher(HandlerMappings mappings)
        {
            this.mappings = mappings;
            log = LogManager.GetLogger(GetType());
        }

        public bool Dispatch(ResolvedEvent @event)
        {
            Type type;
            var clrEventType = @event.Event.EventType;

            if (mappings.TryResolveMapping(clrEventType, out type))
            {
                var serializer = new JsonStringSerializer();
                var typedEvent = serializer.DeserializeFromString(@event.Event.Data.FromAsciiBytes(), type);

                foreach (var handlerType in mappings.GetHandlersForEvent(type))
                {
                    try
                    {
                        dynamic handler = container.TryResolve(handlerType);
                        handler.Handle((dynamic) typedEvent);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message);
                    }
                }
                return true;
            }
            return false;
        }
    }
}

