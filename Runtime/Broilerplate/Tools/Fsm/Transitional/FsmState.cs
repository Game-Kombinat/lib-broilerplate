namespace Broilerplate.Tools.Fsm.Transitional {
    public delegate void StateFunc();
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