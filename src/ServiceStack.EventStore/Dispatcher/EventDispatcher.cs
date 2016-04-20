// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.EventStore.Dispatcher
{
    using System;
    using Funq;
    using Text;
    using Logging;
    using global::EventStore.ClientAPI;
    using System.Threading.Tasks;
    using Host;
    using Events;
    using Extensions;

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

        public async Task Dispatch(ResolvedEvent @event)
        {
            var headers = JsonObject.Parse(@event.Event.Metadata.FromAsciiBytes()).ToNameValueCollection();
            var clrEventType = headers.Get(EventClrTypeHeader);

            Type type;

            if (EventTypes.TryResolveMapping(clrEventType, out type))
            {
                var typedEvent = JsonSerializer.DeserializeFromString(@event.Event.Data.FromAsciiBytes(), type);

                try
                {
                    var request = new BasicRequest();
                    request.Headers.AddAll(headers);

                    await HostContext.ServiceController.ExecuteAsync(typedEvent, request);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
                return;
            }
        }
    }
}

