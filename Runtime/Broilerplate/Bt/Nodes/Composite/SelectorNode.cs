using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.ControlFlow.Bt;

namespace Broilerplate.Bt.Nodes.Composite {
    /// <summary>
    /// Runs until one of the connected nodes return a success.
    /// If none return success, a failure is returned at the end
    /// </summary>
    [NodeTint(NodeColors.Blue)]
    [CreateNodeMenu("Composite/Selector (Run until success)")]
    public class SelectorNode : NodeWithChildren<Port> {

        private int activeChildIndex;
        private BaseNode activeChild;

        protected override void InternalSpawn() {
            activeChildIndex = 0;
            activeChild = childNodes[activeChildIndex];
            activeChild.Spawn();
        }

        protected  override TaskStatus InternalTick() {
            TaskStatus childStatus = activeChild.Status;

            // Check for termination states first
            if (childStatus == TaskStatus.Running) {
                return TaskStatus.Running;
            }
            if (childStatus == TaskStatus.Success) {
                return TaskStatus.Success;
            }
            
            // Check if we reached the selectors end
            if (activeChildIndex == childNodes.Count - 1) {
                // No child returned success. This was a fail.
                return TaskStatus.Failure;
            }
            
            // Still here? Lets keep on going with the next child
            activeChildIndex++;
            activeChild = childNodes[activeChildIndex];
            activeChild.Spawn();
            return TaskStatus.Running;
        }

        protected override void InternalTerminate() {
            activeChildIndex = 0;
            if (activeChild != null) {
                activeChild.Terminate();
            }
        }
    }
}