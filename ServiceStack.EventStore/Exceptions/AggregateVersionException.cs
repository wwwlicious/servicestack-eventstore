namespace ServiceStack.EventStore.Exceptions
{
    using System;

    /// <summary>
    /// An exception thrown when trying persist new events to an 
    /// (aggregate) stream but there have been other changes in the meantime.
    /// </summary>
    public class AggregateVersionException : Exception
    {
        public readonly Guid Id;
        public readonly Type Type;
        public readonly int AggregateVersion;
        public readonly int RequestedVersion;
        
        public AggregateVersionException(Guid id, Type type, int aggregateVersion, int requestedVersion)
            : base(string.Format($"Requested version {requestedVersion} of aggregate '{id}' (type {type.Name}) - aggregate version is {aggregateVersion}"))
        {
            Id = id;
            Type = type;
            AggregateVersion = aggregateVersion;
            RequestedVersion = requestedVersion;
        }
    }
}