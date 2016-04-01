// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

namespace ServiceStack.EventStore.Resilience
{
    using System;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Polly;
    using Types;
    using Logging;

    /// <summary>
    /// Defines the policy for dealing with dropped subscriptions
    /// </summary>
    internal static class DroppedSubscriptionPolicy
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DroppedSubscriptionPolicy));

        /// <summary>
        /// Handles a dropped subscription according to a specified policy.
        /// </summary>
        /// <param name="streamId">The stream to which the subscription was dropped.</param>
        /// <param name="dropReason">The reason why the subscription was dropped.</param>
        /// <param name="exceptionMessage">The exception message provided by EventStore.</param>
        /// <param name="compensatingAction">The action to take in order to handle the subscription being dropped.</param>
        /// <returns></returns>
        public static async Task Handle(DroppedSubscription subscription, Func<Task> compensatingAction)
        {
            var message = subscription.ExceptionMessage;
            var retryPolicy = subscription.RetryPolicy;

            switch (subscription.DropReason)
            {
                case SubscriptionDropReason.UserInitiated:
                    //the client called Close() on the subscription
                    log.Info($@"Subscription to {subscription.StreamId} was closed by the client. 
                                {message}");
                    break;
                case SubscriptionDropReason.NotAuthenticated:
                    //the client is not authenticated -> check ACL
                    log.Error($@"Subscription to {subscription.StreamId} was dropped because the client could not be authenticated. 
                                 Check the Access Control List. {message}");
                    break;
                case SubscriptionDropReason.AccessDenied:
                    //access to the stream was denied -> check ACL
                    log.Error($@"Subscription to {subscription.StreamId} was dropped because the client was denied access. 
                                 Check the Access Control List. {message}");
                    break;
                case SubscriptionDropReason.SubscribingError:
                    //something went wrong while subscribing - retry
                    log.Error($@"Subscription to {subscription.StreamId} failed. 
                                 {message}");
                    await RetrySubscriptionAsync(compensatingAction, retryPolicy).ConfigureAwait(false);
                    break;
                case SubscriptionDropReason.ServerError:
                    //error on the server
                    log.Error($@"A server error occurred which dropped the subscription to {subscription.StreamId}
                                {message}");
                    break;
                case SubscriptionDropReason.ConnectionClosed:
                    //the connection was closed - retry
                    log.Error($@"Subscription to {subscription.StreamId} was dropped due to the connection being closed.
                                 {message}");
                    await RetrySubscriptionAsync(compensatingAction, retryPolicy).ConfigureAwait(false);
                    break;
                case SubscriptionDropReason.CatchUpError:
                    //an error occurred during the catch-up phase - retry
                    log.Error($@"Subscription to {subscription.StreamId} was dropped during the catch-up phase.
                                 {message}");
                    await RetrySubscriptionAsync(compensatingAction, retryPolicy).ConfigureAwait(false);
                    break;
                case SubscriptionDropReason.ProcessingQueueOverflow:
                    //occurs when the number of events on the push buffer exceed the specified maximum - retry
                    log.Warn($@"Subscription to {subscription.StreamId} was dropped due to a processing buffer overflow.
                                {message}");
                    await RetrySubscriptionAsync(compensatingAction, retryPolicy).ConfigureAwait(false);
                    break;
                case SubscriptionDropReason.EventHandlerException:
                    //Subscription dropped because an exception was thrown by one of our handlers.
                    log.Error($@"Subscription to {subscription.StreamId} was dropped in response to a handler exception.
                                 {message}");
                    break;
                case SubscriptionDropReason.MaxSubscribersReached:
                    //The maximum number of subscribers for the persistent subscription has been reached
                    log.Error($@"Subscription to {subscription.StreamId} was dropped because the maximum no. of subscribers was reached.
                                 {message}");
                    break;
                case SubscriptionDropReason.PersistentSubscriptionDeleted:
                    //The persistent subscription has been deleted
                    log.Error($@"The persistent subscription to {subscription.StreamId} was dropped because it was deleted.
                                 {message}");
                    break;
                case SubscriptionDropReason.Unknown:
                    //Scoobied
                    log.Error($@"Subscription to {subscription.StreamId} was dropped for an unspecified reason.
                                 {message}");
                    break;
                case SubscriptionDropReason.NotFound:
                    //Target of persistent subscription was not found. Needs to be created first
                    log.Error($@"The persistent subscription to {subscription.StreamId} could not be found. 
                                 {message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subscription.DropReason), subscription.DropReason, null);
            }
        }

        private static Task RetrySubscriptionAsync(Func<Task> compensatingAction, RetryPolicy retryPolicy)
        {
            if (retryPolicy.RetryType == RetryType.Provider)
            {
                return Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync((int) retryPolicy.MaxNoOfRetries, retryPolicy.SleepDurationProvider)
                    .ExecuteAsync(compensatingAction);
            }
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryPolicy.SleepDurations)
                .ExecuteAsync(compensatingAction);
        }
    }
}
