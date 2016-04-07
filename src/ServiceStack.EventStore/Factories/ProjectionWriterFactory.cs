// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.Factories
{
    using HelperClasses;
    using Projections;
    using Redis;

    /// <summary>
    /// A factory class for creating instances of IProjectionWriter
    /// </summary>
    public static class ProjectionWriterFactory
    {
        public static IProjectionWriter<TId, TViewModel> GetRedisClient<TId, TViewModel>() where TViewModel : class
            where TId : struct
        {
            var redisClientManager = HostContext.Container.Resolve<IRedisClientsManager>();
            return New<RedisProjectionWriter<TId, TViewModel>>
                    .WithCtorParam<IRedisClientsManager>.Instance(redisClientManager);
        }
    }
}
