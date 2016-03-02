using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Publisher
{
    using Types;
    using Logging;
    using Maclean.DeterministicGuids;
    using Resilience;
    using System.Reflection;

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

        public void Publish(Event @event)
        {
            var json = @event.ToJson();
            var assembly = Assembly.GetExecutingAssembly();
            var deterministicId = GuidUtility.Create(assembly.GetType().GUID, json);
            var streamId = $"{@event.StreamName}-{deterministicId}";

            circuitBreaker.Execute(() =>
            {
                connection.AppendToStreamAsync(streamId, ExpectedVersion.Any,
                        new EventData(deterministicId, @event.GetType().Name, true, json.ToAsciiBytes(), new byte[] { }));
            });

            log.Info($"Logged event: {@event}");
        }
    }
}
