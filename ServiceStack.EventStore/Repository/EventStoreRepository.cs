using EventStore.ClientAPI;

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

    public delegate string GetStreamName(Type type, Guid guid);

    //todo: add ability to save and load snapshots
    public class EventStoreRepository : IEventStoreRepository
    {
        private const string EventClrTypeHeader = "EventClrTypeName";
        private const string AggregateClrTypeHeader = "AggregateClrTypeName";
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;

        private readonly GetStreamName getStreamName;
        private readonly ILog log;

        public EventStoreRepository(IEventStoreConnection connection)
        {
            Connection = connection;
            getStreamName = (type, guid) => $"{type.Name}-{guid}"; //todo make this a delegate
            log = LogManager.GetLogger(GetType());
        }

        public IEventStoreConnection Connection { get; }

        public async void Publish(Event @event)
        {
            var streamName = @event.StreamName;

            var headers = new Dictionary<string, object>
                {
                    {EventClrTypeHeader, @event.GetType().Name}
                };

            await Connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, ToEventData(@event, headers));
        }

        public async Task Save(Aggregate aggregate)
        {
            var headers = new Dictionary<string, object>
                {
                    {AggregateClrTypeHeader, aggregate.GetType().Name}
                };

            var streamName = getStreamName(aggregate.GetType(), aggregate.Id);

            var newEvents = aggregate.Changes.ToList();
            var originalVersion = aggregate.State.Version - newEvents.Count;
            var expectedVersion = originalVersion == 0
                                    ? ExpectedVersion.NoStream
                                    : originalVersion - 1;

            var eventsToSave = newEvents.Select(@event => ToEventData(@event, headers)).ToList();

            if (eventsToSave.Count < WritePageSize)
            {
                try
                {
                    await Connection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
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
            }
            else
            {
                var transaction = await Connection.StartTransactionAsync(streamName, expectedVersion);
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
            aggregate.ClearCommittedEvents();
        }
        public async Task<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return await GetById<TAggregate>(id, int.MaxValue);
        }

        //todo: there is a question about an AggregateCreated being raised when the aggregate is created for the first time
        //todo: we would probably want to ignore it when rebuilding the state of an aggregate. However, if we start the slice at
        //todo: position 1 then this would be a problem if no AggregateCreated event was raised
        public async Task<TAggregate> GetById<TAggregate>(Guid id, int version) where TAggregate : Aggregate
        {
            if (version <= 0)
                throw new InvalidOperationException("Cannot get version <= 0");

            var streamName = getStreamName(typeof(TAggregate), id);

            var aggregate = ConstructAggregate<TAggregate>(id);

            var sliceStart = 0; 
            StreamEventsSlice currentSlice;

            do
            {
                var sliceCount = sliceStart + ReadPageSize <= version
                                    ? ReadPageSize
                                    : version - sliceStart + 1;

                currentSlice = await Connection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false);

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
                    aggregate.ApplyEvent((IDomainEvent) DeserializeEvent(evnt.OriginalEvent.Metadata, evnt.OriginalEvent.Data));
            } while (version >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            if (aggregate.State.Version != version && version < int.MaxValue)
                throw new AggregateVersionException(id, typeof (TAggregate), aggregate.State.Version, version);

            return aggregate;
        }

        private static object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JsonObject.Parse(metadata.FromAsciiBytes()).GetUnescaped(EventClrTypeHeader);
            var serializer = new JsonStringSerializer();

            return serializer.DeserializeFromString(data.FromAsciiBytes(), Type.GetType(eventClrTypeName));
        }

        private static TAggregate ConstructAggregate<TAggregate>(Guid id)
        {
            return (TAggregate) Activator.CreateInstance(typeof (TAggregate), id);
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
                    EventClrTypeHeader, @event.GetType().AssemblyQualifiedName
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
