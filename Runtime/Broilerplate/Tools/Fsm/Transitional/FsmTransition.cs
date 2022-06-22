using System;

namespace Broilerplate.Tools.Fsm.Transitional {
    internal class FsmTransition<TStateId> {
        public Func<bool> Condition { get; }
        public FsmState<TStateId> To { get; }

        public FsmTransition(FsmState<TStateId> to, Func<bool> condition) {
            To = to;
            Condition = condition;
        }
    }
}