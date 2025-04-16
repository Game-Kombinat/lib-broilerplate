using System;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Very simply returns Failure or Success based on a condition that was given in the constructor.
    /// </summary>
    public class Condition : Node {
        private readonly Func<bool> predicate;
        public Condition(string name, Func<bool> condition) : base(name) {
            predicate = condition;
        }

        protected override TaskStatus Process() {
            return predicate() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}