// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Repository
{
    using Types;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
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
    using EventTypes = Events.EventTypes;

    public delegate string GetStreamName(Type type, Guid guid);

    // todo: add ability to save and load snapshots
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
        private readonly IEventStoreConnection connection;

        private readonly GetStreamName getStreamName;
        private readonly ILog log;
        private readonly Dictionary<ReadDirection, Func<string, int, int, Task<StreamEventsSlice>>> eventStreamReaders;

        public EventStoreRepository(IEventStoreConnection connection)
        {
            this.connection = connection;
            getStreamName = (type, guid) => $"{type.Name}-{guid}"; // todo make this a delegate
            log = LogManager.GetLogger(GetType());
            eventStreamReaders = new Dictionary<ReadDirection, Func<string, int, int, Task<StreamEventsSlice>>>()
            {
                [ReadDirection.Forwards] = async (s, i, c) => await connection.ReadStreamEventsForwardAsync(s, i, c, resolveLinkTos: true).ConfigureAwait(false),
                [ReadDirection.Backwards] = async (s, i, c) => await connection.ReadStreamEventsBackwardAsync(s, i, c, true).ConfigureAwait(false)
            };
        }

        /// <summary>
        /// Reads a slice of events from a named stream.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to be returned.</typeparam>
        /// <param name="streamName">The name of the stream name to read from.</param>
        /// <param name="direction">The direction in which to read the stream - forward or backward. </param>
        /// <param name="start">The position in the stream of the first event in the requested slice.</param>
        /// <param name="count">The number of events to be included in the requested slice. The maximum number events that can be requested is 4,096.</param>
        /// <returns>A Task of IEnumerable of TEvent</returns>
        public async Task<IEnumerable<TEvent>> ReadSliceFromStreamAsync<TEvent>(string streamName, ReadDirection direction, int start, int count)
        {
            var currentSlice = await eventStreamReaders[direction].Invoke(streamName, start, count).ConfigureAwait(false);
            var events = currentSlice.Events;

            return events.Select(e => DeserializeEvent(e.OriginalEvent.Metadata, e.OriginalEvent.Data).ConvertTo<TEvent>());
        }

        /// <summary>
        /// Reads a slice of events from a named stream.
        /// </summary>
        /// <param name="streamName">The name of the stream name to read from.</param>
        /// <param name="direction">The direction in which to read the stream - forward or backward. </param>
        /// <param name="start">The position in the stream of the first event in the requested slice.</param>
        /// <param name="count">The number of events to be included in the requested slice. The maximum number events that can be requested is 4,096.</param>
        /// <returns>A Task of IEnumerable of object</returns>
        public async Task<IEnumerable<object>> ReadSliceFromStreamAsync(string streamName, ReadDirection direction, int start, int count)
        {
            var currentSlice = await eventStreamReaders[direction].Invoke(streamName, start, count).ConfigureAwait(false);
            var events = currentSlice.Events;

            return events.Select(e => DeserializeEvent(e.OriginalEvent.Metadata, e.OriginalEvent.Data));
        }

        public async Task<TEvent> ReadEventAsync<TEvent>(string streamName, int position) where TEvent: class
        {
            var result = await connection.ReadEventAsync(streamName, position, resolveLinkTos: true).ConfigureAwait(false);
           
            if (!result.Event.HasValue)
                throw new EventNotFoundException(streamName, position);

            var originalEvent = result.Event.Value.OriginalEvent;

            var evt = DeserializeEvent(originalEvent.Metadata, originalEvent.Data);

            return evt as TEvent;
        }

        public async Task DeleteStreamAsync(string streamName, int expectedVersion, bool hardDelete = false)
        {
            await connection.DeleteStreamAsync(streamName, expectedVersion, hardDelete).ConfigureAwait(false);
        }

        public async Task PublishAsync<T>(T @event, string streamName, Action<IDictionary<string, object>> updateHeaders = null)
        {
            var headers = new Dictionary<string, object>();

            updateHeaders?.Invoke(headers);

            try
            {
                await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, ToEventData(@event, headers));
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

            var events = newEvents.Select(@event => (EventData) ToEventData(@event, headers));
            var eventToSave = events as IList<EventData> ?? events.ToList();

            if (eventToSave.Count < WritePageSize)
            {
                try
                {
                    await connection.AppendToStreamAsync(streamName, expectedVersion, eventToSave);
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
                    using (var transaction = await connection.StartTransactionAsync(streamName, expectedVersion))
                    {
                        var position = 0;

                        while (position < eventToSave.Count)
                        {
                            var pageEvents = eventToSave.Skip(position).Take(WritePageSize);
                            try
                            {
                                await transaction.WriteAsync(pageEvents);
                            }
                            catch (Exception e) when (e.Message.Contains("WrongExpectedVersion"))
                            {
                                log.Error(e);
                                // todo: throw appropriate exception e.g. AggregateVersionException
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

        public Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate => 
            GetByIdAsync<TAggregate>(id, int.MaxValue);

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

                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false)
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
                    aggregate.ApplyEvent(DeserializeEvent(evnt.OriginalEvent.Metadata, evnt.OriginalEvent.Data));

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

        private static TAggregate ConstructAggregate<TAggregate>(Guid id) where TAggregate : Aggregate => 
            New<TAggregate>.WithCtorParam<Guid>.Instance(id);

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

        // todo: this will be changed to stop using the assembly guid
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
