namespace ServiceStack.EventStore.Types
{
    using System.Collections.Generic;

    public abstract class EventStoreEvent
    {
        protected Dictionary<string, object> metadata = new Dictionary<string, object>();

        public EventStoreEvent AddMetadata(string key, object value)
        {
            metadata.Add(key, value);
            return this;
        }

        public IDictionary<string, object> GetMetadata()
        {
            return metadata;
        }
    }
}
