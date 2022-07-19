using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.ControlFlow.Bt;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.FloatPort)]
    [CreateNodeMenu("Data Context/Set Float Value")]
    public class SetFloatNode : SingleGraphValueSetterNode {
        [Input(typeConstraint = TypeConstraint.InheritedInverse)]
        private NbtFloatPort inputValue = null;
        
        [SerializeField]
        private float @default;
        
        protected override TaskStatus InternalTick() {
            // Could require a float node here but I also want to allow integers.
            // So we do that and then it will explode in the FloatValue getter instead of GetInputValue.
            // It will explode regardless. As it should be.
            var nbtValue = GetInputValue<NbtTag>(nameof(inputValue));
            if (nbtValue == null) {
                GetDataContextFromScope().Set(varName, @default);
            }
            else {
                GetDataContextFromScope().Set(varName, nbtValue.FloatValue);
            }
            
            return TaskStatus.Success;
        }
        
        public float GetDefaultValue() {
            return @default;
        }

        public void SetDefaultValue(float value) {
            @default = value;
        }
    }
}