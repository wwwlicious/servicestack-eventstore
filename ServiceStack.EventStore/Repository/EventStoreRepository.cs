using ServiceStack.EventStore.EventTypeManagement;

namespace ServiceStack.EventStore.Repository
{
    using Types;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Idempotency;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Exceptions;
    using Text;
    using Logging;
    using Extensions;
    using HelperClasses;
    using global::EventStore.ClientAPI;
    using EventTypes = EventTypes;

    public delegate string GetStreamName(Type type, Guid guid);

    //todo: add ability to save and load snapshots
    /// <summary>
    /// Based on: https://github.com/EventStore/getting-started-with-event-store/blob/master/src/GetEventStoreRepository/GetEventStoreRepository.cs
    /// </summary>
    public class EventStoreRepository : IEventStoreRepository
    {
        private const string EventClrTypeHeader = "EventClrTypeName";
        private const string CausedBy = "$causedBy";
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private const int InitialVersion = 0;

        private readonly GetStreamName getStreamName;
        private readonly ILog log;

        public EventStoreRepository(IEventStoreConnection connection)
        {
            Connection = connection;
            getStreamName = (type, guid) => $"{type.Name}-{guid}"; //todo make this a delegate
            log = LogManager.GetLogger(GetType());
        }

        public IEventStoreConnection Connection { get; }

        public async Task PublishAsync(Event @event, Action<IDictionary<string, object>> updateHeaders = null)
        {
            var streamName = @event.StreamName;

            var headers = new Dictionary<string, object>();

            updateHeaders?.Invoke(headers);

            try
            {
                await Connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, ToEventData(@event, headers));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        public async Task SaveAsync(Aggregate aggregate, Action<IDictionary<string, object>> updateHeaders = null) 
        {
            var headers = new Dictionary<string, object>();

            updateHeaders?.Invoke(headers);

            var streamName = getStreamName(aggregate.GetType(), aggregate.Id);

            var newEvents = aggregate.Changes.ToList();
            var originalVersion = aggregate.State.Version - newEvents.Count;
            var expectedVersion = originalVersion == InitialVersion
                                    ? ExpectedVersion.NoStream
                                    : originalVersion.Subtract(1);

            var eventsToSave = newEvents.Select(@event => ToEventData(@event, headers)).ToList();

            if (eventsToSave.Count < WritePageSize)
            {
                try
                {
                    await Connection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
                }
                catch (Exception exception)
                { 
                    log.Error(exception);
                }
            }
            else
            {
                try
                {
                    using (var transaction = await Connection.StartTransactionAsync(streamName, expectedVersion))
                    {
                        var position = 0;

                        while (position < eventsToSave.Count)
                        {
                            var pageEvents = eventsToSave.Skip(position).Take(WritePageSize);
                            try
                            {
                                await transaction.WriteAsync(pageEvents);
                            }
                            catch (Exception e) when (e.Message.Contains("WrongExpectedVersion"))
                            {
                                log.Error(e);
                                //todo: throw appropriate exception e.g. AggregateVersionException
                            }
                            catch (Exception e)
                            {
                                log.Error(e);
                            }
                            position += WritePageSize;
                        }
                        await transaction.CommitAsync();
                    }
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
            aggregate.ClearCommittedEvents();
        }

        public Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return GetByIdAsync<TAggregate>(id, int.MaxValue);
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version) where TAggregate : Aggregate
        {
            if (version < InitialVersion)
                throw new InvalidOperationException($"Cannot get version < {InitialVersion}");

            var streamName = getStreamName(typeof(TAggregate), id);

            var aggregate = ConstructAggregate<TAggregate>(id);

            var sliceStart = 0; 
            StreamEventsSlice currentSlice;

            do
            {
                var sliceCount = sliceStart + ReadPageSize <= version
                                    ? ReadPageSize
                                    : version - sliceStart;

                currentSlice = await Connection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false)
                                               .ConfigureAwait(false);

                switch (currentSlice.Status)
                {
                    case SliceReadStatus.StreamNotFound:
                        throw new AggregateNotFoundException(id, typeof(TAggregate));
                    case SliceReadStatus.StreamDeleted:
                        throw new AggregateDeletedException(id, typeof(TAggregate));
                    case SliceReadStatus.Success:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                sliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                    aggregate.ApplyEvent((IAggregateEvent) DeserializeEvent(evnt.OriginalEvent.Metadata, evnt.OriginalEvent.Data));

            } while (version > currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            if (aggregate.State.Version != version && version < int.MaxValue)
                throw new AggregateVersionException(id, typeof (TAggregate), aggregate.State.Version, version);

            return aggregate;
        }

        private static object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JsonObject.Parse(metadata.FromAsciiBytes()).GetUnescaped(EventClrTypeHeader);
            Type type;
            if (!EventTypes.TryResolveMapping(eventClrTypeName, out type))
                throw new InvalidOperationException($"Could not resolve event type {eventClrTypeName}");

            return JsonSerializer.DeserializeFromString(data.FromAsciiBytes(), type);
        }

        private static TAggregate ConstructAggregate<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return New<TAggregate>.WithCtorParam<Guid>.Instance(id);
        }

        private EventData ToEventData(object @event, IDictionary<string, object> headers)
        {
            var json = @event.ToJson();
            var data = json.ToAsciiBytes();
            var deterministicEventId = GetDeterministicEventId(json);
            var typeName = @event.GetType().Name;

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    EventClrTypeHeader, @event.GetType().Name
                }
            };

            var metadata = eventHeaders.ToJson().ToAsciiBytes();

            return new EventData(deterministicEventId, typeName, true, data, metadata);
        }

        //todo: this will be changed to stop using the assembly guid
        private Guid GetExecutingAssemblyGuid()
        {
            var assembly = GetType().Assembly;
            var attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof (GuidAttribute), true)[0];
            var assemblyGuid = new Guid(attribute.Value);
            return assemblyGuid;
        }

        private Guid GetDeterministicEventId(string json)
        {
            var assemblyGuid = GetExecutingAssemblyGuid();
            var deterministicEventId = GuidUtility.Create(assemblyGuid, json);
            return deterministicEventId;
        }
    }
}
