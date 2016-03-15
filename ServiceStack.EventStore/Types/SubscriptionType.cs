namespace ServiceStack.EventStore.Types
{
    /// <summary>
    /// Represents the three possible types of sunscription to EventStore.
    /// </summary>
    public enum SubscriptionType
    {
        Persistent, 
        CatchUp, 
        Volatile
    }
}
