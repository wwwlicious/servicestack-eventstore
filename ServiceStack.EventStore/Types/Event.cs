namespace ServiceStack.EventStore.Types
{
    public abstract class Event
    {
        public string StreamName { get; }

        protected Event(string streamName)
        {
            StreamName = streamName;
        }
    }
}
