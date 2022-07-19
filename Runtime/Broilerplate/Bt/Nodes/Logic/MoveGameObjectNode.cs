using GameKombinat.ControlFlow.Bt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Logic {
    [NodeTint(NodeColors.Green)]
    [CreateNodeMenu("Logic/Teleport Game Object")]
    public class MoveGameObjectNode : BaseNode {

        public GameObject toMove;

        public Transform moveTarget;
        
        protected override void InternalSpawn() {
            // nothing to do
        }

        protected override TaskStatus InternalTick() {
            toMove.transform.position = moveTarget.position;
            return TaskStatus.Success;
        }

        protected override void InternalTerminate() {
            // nothing to do
        }
    }
}