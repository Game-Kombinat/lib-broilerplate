using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes.Composite {
    /// <summary>
    /// Runs all child nodes until a failure is returned, then ends execution.
    /// </summary>
    [NodeTint(NodeColors.Blue)]
    [CreateNodeMenu("Composite/Sequence (Run until failure)")]
    public class SequenceNode : NodeWithChildren<Port> {
        private int activeChildIndex;
        private BaseNode activeChild;

        protected override void InternalSpawn() {
            base.InternalSpawn();
            activeChildIndex = 0;
            activeChild = childNodes[activeChildIndex];
            activeChild.Spawn();
        }

        protected override TaskStatus InternalTick() {
            TaskStatus childStatus = activeChild.Status;

            if (childStatus == TaskStatus.Running) {
                return TaskStatus.Running;
            }
            if (childStatus == TaskStatus.Failure || childStatus == TaskStatus.Terminated) {
                return TaskStatus.Failure;
            }
            if (activeChildIndex == childNodes.Count - 1) {
                return TaskStatus.Success;
            }
            
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