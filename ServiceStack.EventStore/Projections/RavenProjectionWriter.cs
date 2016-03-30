namespace ServiceStack.EventStore.Projections
{
    using System;
    using System.Threading.Tasks;

    class RavenProjectionWriter<TId, TViewModel> : IProjectionWriter<TId, TViewModel> 
                                                    where TId : struct 
                                                    where TViewModel : class
    {
        public RavenProjectionWriter()
        {
            
        }
        public Task Add(TViewModel item)
        {
            throw new NotImplementedException();
        }

        public Task Update(TId id, Action<TViewModel> update)
        {
            throw new NotImplementedException();
        }
    }
}
