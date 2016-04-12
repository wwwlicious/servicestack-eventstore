namespace ServiceStack.EventStore.Extensions
{
    using System.Collections.Specialized;
    using Web;

    internal static class CollectionExtensions
    {
        public static void AddAll(this INameValueCollection @this, NameValueCollection collection)
        {
            foreach (var key in collection.AllKeys)
            {
                @this.Add(key, collection[key]);
            }
        }
    }
}
