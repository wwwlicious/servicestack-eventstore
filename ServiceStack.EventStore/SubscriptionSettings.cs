namespace ServiceStack.EventStore
{
    using System;
    using System.Collections.Generic;
    using Subscriptions;

    /// <summary>
    /// Settings required to manage subscriptions to EventStore.
    /// </summary>
    public class SubscriptionSettings
    {
        private string deadLetterChannel = "deadletters";
        private string subscriptionGroup = "consumer-group";
        private string invalidMessageChannel = "invalidmessages";
        private int noOfSubscriptionRetries = 5;

        private IList<Subscription> subscriptions { get; } = new List<Subscription>();

        public IReadOnlyList<Subscription> Subscriptions => subscriptions as IReadOnlyList<Subscription>;

        public SubscriptionSettings SubscribeToStreams(Action<IList<Subscription>> updateSubscriptions = null)
        {
            updateSubscriptions?.Invoke(subscriptions);
            return this;
        }

        public string SubscriptionGroup()
        {
            return subscriptionGroup;
        }

        public SubscriptionSettings SubscriptionGroup(string @group)
        {
            subscriptionGroup = @group;
            return this;
        }

        /// <summary>
        /// Gets the maximum number of specified retries for a subscription that has dropped.
        /// </summary>
        /// <returns>The maximum number of times to retry a subscription.</returns>
        public int MaxSubscriptionRetries()
        {
            return noOfSubscriptionRetries;
        }

        /// <summary>
        /// Builder method to specify the maximum number of times to retry a subscription that has dropped.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns>An instance of SubscriptionSettings</returns>
        public SubscriptionSettings MaxSubscriptionRetries(int retries)
        {
            noOfSubscriptionRetries = retries;
            return this;
        }

        /// <summary>
        /// Get the name of the stream to which invalid messages are written.
        /// </summary>
        /// <returns>Stream Id</returns>
        public string InvalidMessageChannel()
        {
            return invalidMessageChannel;
        }

        /// <summary>
        /// Builder method to set the name of the stream to which invalid messages are to be written
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>In instance of SubscriptionSettings</returns>
        public SubscriptionSettings InvalidMessageChannel(string channel)
        {
            invalidMessageChannel = channel;
            return this;
        }

        public string DeadLetterChannel()
        {
            return deadLetterChannel;
        }

        public SubscriptionSettings DeadLetterChannel(string channel)
        {
            deadLetterChannel = channel;
            return this;
        }
    }
}