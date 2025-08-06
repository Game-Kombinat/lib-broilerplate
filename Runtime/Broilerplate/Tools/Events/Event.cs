using System;

namespace Broilerplate.Tools.Events {
    public class Event<TImplementor> : IEvent where TImplementor : Event<TImplementor>, IEvent {
        /// <summary>
        /// Raise this this event on the EventDispatcher.
        /// </summary>
        public void Call() {
            EventDispatcher.Instance.Call((TImplementor)this);
        }

        public static void Register(Action<TImplementor> hook) {
            EventDispatcher.Instance.Register(hook);
        }

        public static void Unregister(Action<TImplementor> hook) {
            EventDispatcher.Instance.Unregister(hook);
        }
    }
}