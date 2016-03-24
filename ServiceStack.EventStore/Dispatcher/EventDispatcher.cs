namespace ServiceStack.EventStore.Dispatcher
{
    using System;
    using Funq;
    using Text;
    using Logging;
    using global::EventStore.ClientAPI;

    public class EventDispatcher : IEventDispatcher
    {
        // ReSharper disable once InconsistentNaming
        public Container container = ServiceStackHost.Instance.Container;
        private readonly EventTypes.EventTypes eventTypes;
        private const string EventClrTypeHeader = "EventClrTypeName";

        private ILog log;

        public EventDispatcher(EventTypes.EventTypes eventTypes)
        {
            this.eventTypes = eventTypes;
            log = LogManager.GetLogger(GetType());
        }

        public bool Dispatch(ResolvedEvent @event)
        {
            var jsonObj = JsonObject.Parse(@event.Event.Metadata.FromAsciiBytes());
            var clrEventType = jsonObj.Get(EventClrTypeHeader);

            Type type;

            if (eventTypes.TryResolveMapping(clrEventType, out type))
            {
                var typedEvent = JsonSerializer.DeserializeFromString(@event.Event.Data.FromAsciiBytes(), type);

                try
                {
                    HostContext.ServiceController.Execute(typedEvent);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
                return true;
            }
            return false;
        }
    }
}

