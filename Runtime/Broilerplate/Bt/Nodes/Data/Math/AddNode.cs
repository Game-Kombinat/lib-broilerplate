using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.Fnbt;
using UnityEngine;
using XNode;

namespace Broilerplate.Bt.Nodes.Data.Math {
    [NodeTint(NodeColors.NumberPort)]
    [CreateNodeMenu("Math/Add")]
    public class AddNode : Node {
        [Input(typeConstraint = TypeConstraint.Inherited)]
        [SerializeField]
        private NbtNumberPort numberA = null;
        [Input(typeConstraint = TypeConstraint.Inherited)]
        [SerializeField]
        private NbtNumberPort numberB = null;
        
        [Output]
        [SerializeField]
        private NbtNumberPort result = null;

        private readonly NbtFloat resultData = new NbtFloat();


        public override object GetValue(NodePort port) {
            if (port.fieldName != nameof(result)) {
                return null;
            }
            var numA = GetInputValue<NbtTag>(nameof(numberA));
            var numB = GetInputValue<NbtTag>(nameof(numberB));
            resultData.Value = numA.FloatValue + numB.FloatValue;
            return resultData;
        }
        
    }
}