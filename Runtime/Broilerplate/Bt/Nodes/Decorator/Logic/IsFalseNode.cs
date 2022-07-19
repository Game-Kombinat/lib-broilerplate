using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Decorator.Logic {
    [NodeTint(NodeColors.BooleanPort)]
    [CreateNodeMenu("Logic/Test If False")]
    public class IsFalseNode : BaseNode {
        [Input(typeConstraint = TypeConstraint.Strict)]
        [SerializeField]
        private NbtBooleanPort testValue = null;
        
        [Output(connectionType = ConnectionType.Override)]
        [SerializeField]
        private Port whenTrue = null;
        [Output(connectionType = ConnectionType.Override)]
        [SerializeField]
        private Port whenFalse = null;
        
        private NbtTag testValueTag;
        private BaseNode childWhenTrue, childWhenFalse;

        private BaseNode activeChild;
        private bool isRunning;
        
        protected override void InternalSpawn() {
            testValueTag = GetInputValue<NbtTag>(nameof(testValue));
            childWhenTrue = GetNext(nameof(whenTrue));
            childWhenFalse = GetNext(nameof(whenFalse));
        }

        protected override TaskStatus InternalTick() {
            if (isRunning) {
                return activeChild.Status;
            }

            activeChild = testValueTag.ByteValue == 0 ? childWhenTrue : childWhenFalse;
            activeChild.Spawn();
            isRunning = true;
            return TaskStatus.Running;
        }

        protected override void InternalTerminate() {
            isRunning = false;
            activeChild = null;
        }
    }
}