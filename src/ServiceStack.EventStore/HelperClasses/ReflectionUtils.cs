// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.HelperClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using StateManagement;

    /// <summary>
    /// Helper class for building handlers that mutate the state of our aggregates
    /// Original source:  https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/Helpers/ReflectionUtils.cs
    /// </summary>
    internal static class ReflectionUtils
    {
        public static Dictionary<string, Action<TState, object>> GetStateEventMutators<TState>()
            where TState : IState
        {
            var stateType = typeof(TState);

            var mutateMethods = stateType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                         .Where(m => m.Name.Length >= 2 && m.Name.StartsWith("On") && m.GetParameters().Length == 1 && m.ReturnType == typeof(void))
                                         .ToArray();

            var stateEventMutators = from method in mutateMethods
                                     let eventType = method.GetParameters()[0].ParameterType
                                     select new
                                     {
                                         Name = eventType.Name,
                                         Handler = BuildStateEventMutatorHandler<TState>(eventType, method)
                                     };

            return stateEventMutators.ToDictionary(m => m.Name, m => m.Handler);
        }

        private static Action<TState, object> BuildStateEventMutatorHandler<TState>(Type eventType, MethodInfo method)
            where TState : IState
        {
            var stateParam = Expression.Parameter(typeof(TState), "state");
            var eventParam = Expression.Parameter(typeof(object), "ev");

            var methodCallExpr = Expression.Call(stateParam, method, Expression.Convert(eventParam, eventType));
            var lambda = Expression.Lambda<Action<TState, object>>(methodCallExpr, stateParam, eventParam);

            return lambda.Compile();
        }
    }
}
