namespace Broilerplate.Tools.Events {
    public class Event<TImplementor> : IEvent where TImplementor : IEvent {
        /// <summary>
        /// Raise this this event on the EventDispatcher.
        /// </summary>
        public void Call() {
            EventDispatcher.Instance.Call<TImplementor>(this);
        }

        public static void Register(Callback<TImplementor> handler) {
            EventDispatcher.Instance.Register(handler);
        }

        public static void Unregister(Callback<TImplementor> handler) {
            EventDispatcher.Instance.Unregister(handler);
        }
    }
}