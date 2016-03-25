namespace ServiceStack.EventStore.Resilience
{
    public static class RetryExtensions
    {
        public static Retries Retries(this int input)
        {
            return (Retries) input;
        }
    }
}
