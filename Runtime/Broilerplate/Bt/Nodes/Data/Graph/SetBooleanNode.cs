using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.BooleanPort)]
    [CreateNodeMenu("Data Context/Set Boolean Value")]
    public class SetBooleanNode : SingleGraphValueSetterNode {
        [Input(typeConstraint = TypeConstraint.Strict)]
        private NbtBooleanPort inputValue = null;

        [SerializeField]
        private bool @default;
        
        protected override TaskStatus InternalTick() {
            var nbtValue = GetInputValue<NbtTag>(nameof(inputValue));
            if (nbtValue == null) {
                GetDataContextFromScope().Set(varName, @default);
            }
            else {
                GetDataContextFromScope().Set(varName, nbtValue.ByteValue == 1);
            }
            return TaskStatus.Success;
        }

        public bool GetDefaultValue() {
            return @default;
        }

        public void SetDefaultValue(bool value) {
            @default = value;
        }
    }
}