using System;

namespace ServiceStack.EventStore.Consumers
{
    using Resilience;
    using System.Threading.Tasks;

    public abstract class StreamConsumer
    {
        //set the default RetryPolicy for each subscription - max 5 retries with exponential backoff
        protected RetryPolicy retryPolicy = new RetryPolicy(5.Retries(), 
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public abstract Task ConnectToSubscription(string streamId, string subscriptionGroup);

        public void SetRetryPolicy(RetryPolicy policy)
        {
            retryPolicy = policy;
        }
    }
}
