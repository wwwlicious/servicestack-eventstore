namespace ServiceStack.EventStore.HelperClasses
{
    using System.Collections.Generic;
    using System.Linq;

    public static class IEnumerableExtensions
    {
        public static bool HasAny<T>(this IEnumerable<T> enumerable)
        {
            // ReSharper disable once UseMethodAny.0
            return (enumerable.Count() > 0);
        }
    }
}
