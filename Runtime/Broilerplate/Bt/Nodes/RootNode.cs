using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes {

    public enum RunMode {
        /// <summary>
        /// Execution ends when the tree is traversed through the end.
        /// Useful for sequences.
        /// </summary>
        HoldAtEnd,
        /// <summary>
        /// The tree will be traversed again when the end is reached,
        /// creating a loop. Useful for AI behaviour and stuff that needs to keep running.
        /// </summary>
        Repeat,
    }
    /// <summary>
    /// This is a required node that is the root of the behaviour tree.
    /// </summary>
    [DisallowMultipleNodes]
    [CreateNodeMenu("System/Root")]
    public class RootNode : BaseNode {
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        public Port child;

        public RunMode runMode;
        private BaseNode childNode;

        protected override void InternalSpawn() {
            childNode = GetNext(nameof(child));
            childNode.Spawn();
        }

        protected  override TaskStatus InternalTick() {
            if (childNode.Status != TaskStatus.Running && runMode == RunMode.Repeat) {
                childNode.Terminate();
                childNode.Spawn();
                return TaskStatus.Running;
            }
            return childNode.Status;
        }

        protected override void InternalTerminate() {
            childNode.Terminate();
        }
    }
}