using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore
{
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
