
using System;
using System.Collections.Generic;

namespace CardDuel.UI.Events
{
    public static class UIEventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : IUIEvent
        {
            var type = typeof(T);
            Handlers[type] = (Action<T>)Handlers.GetValueOrDefault(type) + handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IUIEvent
        {
            var type = typeof(T);
            if (Handlers.ContainsKey(type))
                Handlers[type] = (Action<T>)Handlers[type] - handler;
        }

        public static void Publish<T>(T evt) where T : IUIEvent
        {
            if (Handlers.TryGetValue(typeof(T), out var del))
                ((Action<T>)del)?.Invoke(evt);
        }
    }
}
