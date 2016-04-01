// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

namespace ServiceStack.EventStore.Extensions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for working with Task
    /// </summary>
    internal static class TaskExtensions
    {
        //Represents a succesfully completed Task
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}
