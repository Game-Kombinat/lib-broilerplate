namespace Broilerplate.Tools.Fsm {
    public delegate void StateFunc();
    /// <summary>
    /// Is a state that is used in a state machine.
    /// Provides a number of callbacks.
    /// </summary>
    /// <typeparam name="TStateId"></typeparam>
    internal class FsmState<TStateId> {
        public FsmState(TStateId id, StateFunc enter, StateFunc tick, StateFunc lateTick, StateFunc exit) {
            this.id = id;
            this.enter = enter;
            this.tick = tick;
            this.lateTick = lateTick;
            this.exit = exit;
        }

        public readonly TStateId id;
        public readonly StateFunc enter;
        public readonly StateFunc tick;
        public readonly StateFunc lateTick;
        public readonly StateFunc exit;
    }
}