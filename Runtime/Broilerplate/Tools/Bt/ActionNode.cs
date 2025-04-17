using System;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Simply invokes a passed-in action and returns success.
    ///
    /// This is called ActionNode, rather than simply Action, for obvious reasons.
    /// </summary>
    public class ActionNode : Node {
        private Action callback;
        public ActionNode(Action action, string name) : base(name) {
            callback = action;
        }

        protected override TaskStatus Process() {
            callback();
            return TaskStatus.Success;
        }
    }
}