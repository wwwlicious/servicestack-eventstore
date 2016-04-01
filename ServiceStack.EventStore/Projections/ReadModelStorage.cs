namespace ServiceStack.EventStore.Projections
{
    public class ReadModelStorage
    {
        public ReadModelStorage(StorageType storageType, string connectionString)
        {
            StorageType = storageType;
            ConnectionString = connectionString;
        }

        public StorageType StorageType { get; }
        public string ConnectionString { get; }
    }
}
