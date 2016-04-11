// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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
    public interface IReadModelWriter<in TId, TViewModel> where TId: struct 
                                                           where TViewModel : class 
    {
        Task Add(TViewModel item);
        Task Update(TId id, Action<TViewModel> update);
    }
}
