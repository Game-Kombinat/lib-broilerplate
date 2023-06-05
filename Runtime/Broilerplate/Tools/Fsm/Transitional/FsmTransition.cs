using System;

namespace Broilerplate.Tools.Fsm.Transitional {
    /// <summary>
    /// Represents a transition from one state to another, bound to a condition which is
    /// evaluated by the <see cref="TransitionalFsm{TStateId}"/> each time it is being ticked.
    /// </summary>
    /// <typeparam name="TStateId"></typeparam>
    internal class FsmTransition<TStateId> {
        public Func<bool> Condition { get; }
        public FsmState<TStateId> To { get; }

        public FsmTransition(FsmState<TStateId> to, Func<bool> condition) {
            To = to;
            Condition = condition;
        }
    }
}