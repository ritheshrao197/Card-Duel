using System;
using System.Collections.Generic;
using CardDuel.Domain.Events;

namespace CardDuel.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Action<IGameEvent>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = _ => { };

            _handlers[type] += e => handler((T)e);
        }

        public static void Publish(IGameEvent evt)
        {
            var type = evt.GetType();
            if (_handlers.TryGetValue(type, out var action))
                action.Invoke(evt);
        }
    }
}
