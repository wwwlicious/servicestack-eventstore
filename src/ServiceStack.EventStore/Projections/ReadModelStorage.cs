// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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
