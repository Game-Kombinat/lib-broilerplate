using System;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Node that gets a Func which returns a task status.
    /// Basically full custom behaviour.
    /// </summary>
    public class ActionControl : Node {
        private readonly Func<TaskStatus> controlFunc;
        public ActionControl(Func<TaskStatus> func, string name) : base(name) {
            controlFunc = func;
        }

        protected override TaskStatus Process() {
            return controlFunc();
        }
    }
}