using System;
using System.Collections.Generic;

namespace CardDuel.UI.Core
{
    public static class UIEventBus
    {
        private static readonly Dictionary<Type, Action<object>> handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!handlers.ContainsKey(type))
                handlers[type] = _ => { };

            handlers[type] += e => handler((T)e);
        }

        public static void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (handlers.TryGetValue(type, out var action))
                action.Invoke(evt);
        }
    }
}
