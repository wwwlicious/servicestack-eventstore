namespace ServiceStack.EventStore.Types
{
    using global::EventStore.ClientAPI;
    using Resilience;

    public class StreamSubscriptionDropped
    {
        public StreamSubscriptionDropped(string streamId, string exceptionMessage, SubscriptionDropReason dropReason, RetryPolicy retryPolicy)
        {
            StreamId = streamId;
            ExceptionMessage = exceptionMessage;
            DropReason = dropReason;
            RetryPolicy = retryPolicy;
        }

        public string StreamId { get; }
        public string ExceptionMessage { get; }
        public SubscriptionDropReason DropReason { get; }
        public RetryPolicy RetryPolicy { get; }
    }
}
