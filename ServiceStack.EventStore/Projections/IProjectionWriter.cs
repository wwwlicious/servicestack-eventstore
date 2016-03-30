namespace ServiceStack.EventStore.Projections
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface that represents a projection writer
    /// Original source: https://github.com/gnschenker/EventSourcing/blob/master/src/ReadModelBuilder/IProjectionWriter.cs
    /// </summary>
    /// <typeparam name="TId">The type parameter of the unique Id of the view model.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    public interface IProjectionWriter<in TId, TViewModel> where TId: struct 
                                                           where TViewModel : class 
    {
        Task Add(TViewModel item);
        Task Update(TId id, Action<TViewModel> update);
    }
}
