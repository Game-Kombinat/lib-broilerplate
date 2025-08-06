using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Tools.Events {
    // Defines the callback for events
    internal class EventContainer<T> {
        private Action<T> events;
        public void Call(T e) {
            events?.Invoke(e);
        }

        public void Add(Action<T> d) {
            events += d;
        }

        public void Remove(Action<T> d) {
            events -= d;
        }
    }

    /// <summary>
    /// High-performance event pump.
    /// It's a event broadcasting service that will pump large amounts of events
    /// in a very fast fashion through your code. Events are called in no specific order.
    /// There are no targeted events.
    /// </summary>
    public class EventDispatcher {
        private static EventDispatcher instance;

        public static EventDispatcher Instance {
            get {
                if (instance == null) {
                    instance = new EventDispatcher();
                }
                return instance;
            }
        }

        /// <summary>
        /// The registrants.
        /// Maps event type to event handlers.
        /// Max efficiency when calling hooks / events, whatever.
        /// These are actually EventContainer&lt;T&gt;
        /// </summary>
        private readonly Dictionary<Type, object> registrants = new();

        #region API

        /// <summary>
        /// Clears all registered listener callbacks
        /// </summary>
        public void ClearAll() {
            registrants.Clear();
        }

        /// <summary>
        /// Register the specified handler.
        /// The event type on the handler defines when it is called.
        /// If it has an UIShowEvent as argument, it will be called when an UIShowEvent
        /// is passed to the Call() method.
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void Register<T>(Action<T> handler) {
            var paramType = typeof(T);

            if (!registrants.ContainsKey(paramType)) {
                registrants.Add(paramType, new EventContainer<T>());
            }
            var handles = (EventContainer<T>)registrants[paramType];
            handles.Add(handler);
        }

        /// <summary>
        /// Unregister the specified handler from the system
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void Unregister<T>(Action<T> handler) {
            var paramType = typeof(T);

            if (!registrants.TryGetValue(paramType, out var registrant)) {
                return;
            }
            var handlers = (EventContainer<T>)registrant;
            handlers.Remove(handler);
        }

        /// <summary>
        /// Call the specified event.
        /// This will cause all registrants to be called that
        /// have typeof(T) events in their signature and the event data is passed along.
        /// If there is no registrant for the given event then nothing will happen.
        /// </summary>
        /// <param name="e">Event to raise.</param>
        public void Call<T>(T e) {
            if (registrants.TryGetValue(typeof(T), out var d)) {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                ((EventContainer<T>)d).Call(e);
            }
        }

        #endregion API
    }
}