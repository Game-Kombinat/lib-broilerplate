using GameKombinat.ControlFlow.Bt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes {
    [NodeTint(NodeColors.Purple)]
    [CreateNodeMenu("Sub Tree")]
    public class SubTreeNode : BaseNode {
        [SerializeField]
        public BehaviourTree subTree;
        protected override void InternalSpawn() {
            subTree.Begin();
        }

        protected override TaskStatus InternalTick() {
            subTree.Tick();
            return subTree.Status;
        }

        protected override void InternalTerminate() {
            subTree.Terminate();
        }
    }
}