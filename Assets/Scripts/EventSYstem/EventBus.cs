using System;
using System.Collections.Generic;

namespace CardDuel.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Action<object>> _events = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (!_events.ContainsKey(type))
                _events[type] = delegate { };

            _events[type] += e => handler((T)e);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_events.ContainsKey(type)) return;

            _events[type] -= e => handler((T)e);
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (_events.ContainsKey(type))
                _events[type]?.Invoke(eventData);
        }
    }
}
