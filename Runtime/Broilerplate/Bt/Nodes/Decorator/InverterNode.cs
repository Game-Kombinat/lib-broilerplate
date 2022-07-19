using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes.Decorator {
    [NodeTint(NodeColors.Orange)]
    [CreateNodeMenu("Decorator/Inverter")]
    public class InverterNode : BaseNode {
        [Output(connectionType = ConnectionType.Override)]
        public Port decorated;

        private BaseNode decoratedChild;
        
        protected override void InternalSpawn() {
            decoratedChild = GetNext(nameof(decorated));
            decoratedChild.Spawn();
        }

        protected override TaskStatus InternalTick() {
            decoratedChild.Tick();
            switch (decoratedChild.Status) {
                case TaskStatus.Success:
                    return TaskStatus.Failure;
                case TaskStatus.Failure:
                    return TaskStatus.Success;
                default:
                    return TaskStatus.Running;
            }
        }

        protected override void InternalTerminate() {
            if (decoratedChild != null) {
                decoratedChild.Terminate();
            }
        }
    }
}