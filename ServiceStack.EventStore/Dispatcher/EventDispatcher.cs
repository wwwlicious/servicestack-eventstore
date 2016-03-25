namespace ServiceStack.EventStore.Dispatcher
{
    using System;
    using Funq;
    using Text;
    using Logging;
    using EventTypes;
    using global::EventStore.ClientAPI;
    using System.Threading.Tasks;
    using Host;

    public class EventDispatcher : IEventDispatcher
    {
        // ReSharper disable once InconsistentNaming
        public Container container = ServiceStackHost.Instance.Container;
        private readonly EventTypes eventTypes;
        private const string EventClrTypeHeader = "EventClrTypeName";

        private ILog log;

        public EventDispatcher(EventTypes eventTypes)
        {
            this.eventTypes = eventTypes;
            log = LogManager.GetLogger(GetType());
        }

        public async Task<bool> Dispatch(ResolvedEvent @event)
        {
            var jsonObj = JsonObject.Parse(@event.Event.Metadata.FromAsciiBytes());
            var clrEventType = jsonObj.Get(EventClrTypeHeader);

            Type type;

            if (eventTypes.TryResolveMapping(clrEventType, out type))
            {
                var typedEvent = JsonSerializer.DeserializeFromString(@event.Event.Data.FromAsciiBytes(), type);

                try
                {
                    await HostContext.ServiceController.ExecuteAsync(typedEvent, new BasicRequest());
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

