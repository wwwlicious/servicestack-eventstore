namespace ServiceStack.EventStore.HelperClasses
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Handy helpers for working with IEnumerable
    /// </summary>
    internal static class IEnumerableExtensions
    {
        /// <summary>
        /// Can be used instead of .Any() which can take longer than .Count > 0 in some situations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns>Boolean value indicating whether the enumerable contains any elements</returns>
        public static bool HasAny<T>(this IEnumerable<T> enumerable)
        {
            // ReSharper disable once UseMethodAny.0
            return (enumerable.Count() > 0);
        }
    }
}
