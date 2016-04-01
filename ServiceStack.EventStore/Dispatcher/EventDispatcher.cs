using ServiceStack.EventStore.Events;

namespace ServiceStack.EventStore.Dispatcher
{
    using System;
    using Funq;
    using Text;
    using Logging;
    using global::EventStore.ClientAPI;
    using System.Threading.Tasks;
    using Host;

    /// <summary>
    /// Mediator class that transforms a ResolvedEvent from EventStore into a CLR type
    /// </summary>
    internal class EventDispatcher : IEventDispatcher
    {
        // ReSharper disable once InconsistentNaming
        public Container container = ServiceStackHost.Instance.Container;
        private const string EventClrTypeHeader = "EventClrTypeName";

        private ILog log;

        public EventDispatcher()
        {
            log = LogManager.GetLogger(GetType());
        }

        public async Task<bool> Dispatch(ResolvedEvent @event)
        {
            var jsonObj = JsonObject.Parse(@event.Event.Metadata.FromAsciiBytes());
            var clrEventType = jsonObj.Get(EventClrTypeHeader);

            Type type;

            if (EventTypes.TryResolveMapping(clrEventType, out type))
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

