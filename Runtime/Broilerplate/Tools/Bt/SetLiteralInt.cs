using Broilerplate.Data;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Simple node to set a literal int to local blackboard.
    /// </summary>
    public class SetLiteralInt : Node {
        private BlackboardKey targetKey;
        private int targetValue;
        public SetLiteralInt(BlackboardKey key, int value, string name) : base(name) {
            targetKey = key;
            targetValue = value;
        }

        protected override TaskStatus Process() {
            Root.Blackboard.Set(targetKey, targetValue);
            return TaskStatus.Success;
        }
    }
}