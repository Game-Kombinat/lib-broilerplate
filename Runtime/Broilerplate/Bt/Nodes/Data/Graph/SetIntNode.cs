using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.IntPort)]
    [CreateNodeMenu("Data Context/Set Int Value")]
    public class SetIntNode : SingleGraphValueSetterNode{
        [Input(typeConstraint = TypeConstraint.InheritedInverse)]
        private NbtIntPort inputValue = null;
        
        [SerializeField]
        private int @default;
        
        protected override TaskStatus InternalTick() {
            // Could require an int node here but I also want to allow floats.
            // So we do that and then it will explode in the IntValue getter instead of GetInputValue.
            // It will explode regardless. As it should be.
            var nbtValue = GetInputValue<NbtTag>(nameof(inputValue));
            if (nbtValue == null) {
                GetDataContextFromScope().Set(varName, @default);
            }
            else {
                GetDataContextFromScope().Set(varName, nbtValue.IntValue);
            }
            
            return TaskStatus.Success;
        }
        
        public int GetDefaultValue() {
            return @default;
        }

        public void SetDefaultValue(int value) {
            @default = value;
        }
    }
}