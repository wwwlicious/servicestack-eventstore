namespace ServiceStack.EventStore.Types
{
    using System.Collections.Generic;

    public abstract class Event
    {
        public string StreamName { get; set; }

        protected Event(string streamName)
        {
            StreamName = streamName;
        }
    }
}
