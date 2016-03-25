using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using ServiceStack.Logging;

namespace ServiceStack.EventStore.Subscriptions
{
    /// <summary>
    /// Defines the policy for dealing with dropped subscriptions
    /// </summary>
    public static class SubscriptionPolicy
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SubscriptionPolicy));

        /// <summary>
        /// Handles a dropped subscription according to a specified policy.
        /// </summary>
        /// <param name="streamId">The stream to which the subscription was dropped.</param>
        /// <param name="dropReason">The reason why the subscription was dropped.</param>
        /// <param name="exception">The exception returned by EventStore.</param>
        /// <param name="compensatingAction">The action to take in order to handle the subscription being dropped.</param>
        /// <returns></returns>
        public static async Task Handle(string streamId, SubscriptionDropReason dropReason, Exception exception, Func<Task> compensatingAction)
        {
            var exceptionMessage = $"Message: {exception.Message}";
            switch (dropReason)
            {
                case SubscriptionDropReason.UserInitiated:
                    //the client called Close() on the subscription
                    log.Info($@"Subscription to {streamId} was closed by the client. 
                                {exceptionMessage}");
                    break;
                case SubscriptionDropReason.NotAuthenticated:
                    //the client is not authenticated -> check ACL
                    log.Error($@"Subscription to {streamId} was dropped because the client could not be authenticated. 
                                 Check the Access Control List. {exceptionMessage}");
                    break;
                case SubscriptionDropReason.AccessDenied:
                    //access to the stream was denied -> check ACL
                    log.Error($@"Subscription to {streamId} was dropped because the client was denied access. 
                                 Check the Access Control List. {exceptionMessage}");
                    break;
                case SubscriptionDropReason.SubscribingError:
                    //something went wrong while subscribing - retry
                    log.Error($@"Subscription to {streamId} failed. 
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.ServerError:
                    //error on the server
                    log.Error($@"A server error occurred which dropped the subscription to {streamId}
                                {exceptionMessage}");
                    break;
                case SubscriptionDropReason.ConnectionClosed:
                    //the connection was closed - retry
                    log.Error($@"Subscription to {streamId} was dropped due to the connection being closed.
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.CatchUpError:
                    //an error occurred during the catch-up phase - retry
                    log.Error($@"Subscription to {streamId} was dropped during the catch-up phase.
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.ProcessingQueueOverflow:
                    //occurs when the number of events on the push buffer exceed the specified maximum - retry
                    log.Warn($@"Subscription to {streamId} was dropped due to a processing buffer overflow.
                                {exceptionMessage}");
                    await compensatingAction.Invoke();
                    break;
                case SubscriptionDropReason.EventHandlerException:
                    //Subscription dropped because an exception was thrown by one of our handlers.
                    log.Error($@"Subscription to {streamId} was dropped in response to a handler exception.
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.MaxSubscribersReached:
                    //The maximum number of subscribers for the persistent subscription has been reached
                    log.Error($@"Subscription to {streamId} was dropped because the maximum no. of subscribers was reached.
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.PersistentSubscriptionDeleted:
                    //The persistent subscription has been deleted
                    log.Error($@"The persistent subscription to {streamId} was dropped because it was deleted.
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.Unknown:
                    //Scoobied
                    log.Error($@"Subscription to {streamId} was dropped for an unspecified reason.
                                 {exceptionMessage}");
                    break;
                case SubscriptionDropReason.NotFound:
                    //Target of persistent subscription was not found. Needs to be created first
                    log.Error($@"The persistent subscription to {streamId} could not be found. 
                                 {exceptionMessage}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dropReason), dropReason, null);
            }
        }
    }
}
