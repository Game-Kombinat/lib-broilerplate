using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.ControlFlow.Bt;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.StringPort)]
    [CreateNodeMenu("Data Context/Set String Value")]
    public class SetStringNode : SingleGraphValueSetterNode {
        [Input(typeConstraint = TypeConstraint.Inherited)]
        [SerializeField]
        private NbtStringPort inputValue = null;
        
        [SerializeField]
        private string @default;
        
        protected override TaskStatus InternalTick() {
            var nbtValue = GetInputValue<NbtTag>(nameof(inputValue));
            if (nbtValue == null) {
                GetDataContextFromScope().Set(varName, @default);
            }
            else {
                GetDataContextFromScope().Set(varName, nbtValue.StringValue);
            }
            
            return TaskStatus.Success;
        }
        
        public string GetDefaultValue() {
            return @default;
        }

        public void SetDefaultValue(string value) {
            @default = value;
        }
    }
}