using System;
using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.ControlFlow.Bt;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Decorator.Logic {
    
    [Serializable]
    public enum BooleanComparison {
        Equal,
        NotEqual,
    }
    [NodeTint(NodeColors.BooleanPort)]
    [CreateNodeMenu("Logic/Boolean Compare")]
    public class CompareBooleanNode : BaseNode {
        [Input(typeConstraint = TypeConstraint.Strict)]
        [SerializeField]
        private NbtBooleanPort lhs = null;

        [SerializeField]
        private BooleanComparison @is = BooleanComparison.Equal;
        
        [Input(typeConstraint = TypeConstraint.Strict)]
        [SerializeField]
        private NbtBooleanPort rhs = null;

        [Output(connectionType = ConnectionType.Override)]
        [SerializeField]
        private Port whenTrue = Port.portValue;
        [Output(connectionType = ConnectionType.Override)]
        [SerializeField]
        private Port whenFalse = Port.portValue;

        private NbtTag lhsTag, rhsTag;
        private BaseNode childWhenTrue, childWhenFalse;

        private BaseNode activeChild;
        private bool isRunning;
        
        
        protected override void InternalSpawn() {
            lhsTag = GetInputValue<NbtTag>(nameof(lhs));
            rhsTag = GetInputValue<NbtTag>(nameof(rhs));
            childWhenTrue = GetNext(nameof(whenTrue));
            childWhenFalse = GetNext(nameof(whenFalse));
        }

        protected override TaskStatus InternalTick() {
            if (isRunning) {
                return activeChild.Status;
            }

            bool lhsBool = lhsTag.ByteValue == 1;
            bool rhsBool = rhsTag.ByteValue == 1;
            
            switch (@is) {
                case BooleanComparison.Equal:
                    activeChild = lhsBool == rhsBool ? childWhenTrue : childWhenFalse;
                    break;
                case BooleanComparison.NotEqual:
                    activeChild = lhsBool != rhsBool ? childWhenTrue : childWhenFalse;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            activeChild.Spawn();
            isRunning = true;
            return TaskStatus.Running;
        }

        protected override void InternalTerminate() {
            activeChild = null;
            isRunning = false;
        }
    }
}