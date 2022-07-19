using UnityEngine;

namespace Broilerplate.Bt.Nodes {
    [NodeTint(NodeColors.Red)]
    [CreateNodeMenu("Debug/Debug Flow")]
    public class FlowDebug : BaseNode {
        public string displayName;
        public bool returnSuccess;
        protected override void InternalSpawn() {
            Debug.Log("Internal Spawn " + displayName);
        }

        protected override TaskStatus InternalTick() {
            if (returnSuccess) {
                Debug.Log("Return success from " + displayName);
                return TaskStatus.Success;
            }
            Debug.Log("Return failure from " + displayName);
            return TaskStatus.Failure;
        }

        protected override void InternalTerminate() {
            Debug.Log("Internal Terminate " + displayName);
        }
    }
}