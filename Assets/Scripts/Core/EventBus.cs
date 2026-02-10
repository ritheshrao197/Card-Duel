using System;
using System.Collections.Generic;

namespace CardDuel.Core
{
    /// <summary>
    /// Central event system for decoupled communication
    /// Everything important becomes an event
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        /// <summary>
        /// Subscribe to an event type
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event type
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        public static void Publish<T>(T evt) where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                // Create a copy to avoid modification during iteration
                var handlersCopy = new List<Delegate>(handlers);
                foreach (var handler in handlersCopy)
                {
                    ((Action<T>)handler)(evt);
                }
            }
        }

        /// <summary>
        /// Clear all handlers (useful for testing)
        /// </summary>
        public static void Clear()
        {
            _handlers.Clear();
        }
    }

    /// <summary>
    /// Marker interface for all events
    /// Events are facts, never instructions
    /// </summary>
    public interface IEvent { }
}