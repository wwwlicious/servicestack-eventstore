using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Publisher
{
    using Types;
    using Logging;
    using Resilience;
    using Idempotency;
    using System;
    using System.Runtime.InteropServices;

    public class EventPublisher: IPublisher
    {
        private readonly ILog log;
        private readonly ICircuitBreaker circuitBreaker;
        private readonly IEventStoreConnection connection;

        public EventPublisher(ICircuitBreaker circuitBreaker, IEventStoreConnection connection)
        {
            this.circuitBreaker = circuitBreaker;
            this.connection = connection;
            log = LogManager.GetLogger(GetType());
        }

        public void Publish<TId>(AggregateEvent<TId> @event) where TId : struct
        {
            var streamId = $"{@event.StreamName}-{@event.AggregateId}";
            var json = @event.ToJson();
            var assemblyGuid = GetExecutingAssemblyGuid();
            var deterministicEventId = GuidUtility.Create(assemblyGuid, json);

            connection.AppendToStreamAsync(streamId, ExpectedVersion.Any,
                    new EventData(deterministicEventId, @event.GetType().Name, true, json.ToAsciiBytes(), new byte[] {}));

            log.Info($"Logged event: {@event}");
        }

        private Guid GetExecutingAssemblyGuid()
        {
            var assembly = GetType().Assembly;
            var attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof (GuidAttribute), true)[0];
            var assemblyGuid = new Guid(attribute.Value);
            return assemblyGuid;
        }
    }
}
