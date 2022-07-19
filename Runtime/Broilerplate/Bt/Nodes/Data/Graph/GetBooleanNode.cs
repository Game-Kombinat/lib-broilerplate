using Broilerplate.Bt.Nodes.Ports;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.BooleanPort)]
    [CreateNodeMenu("Data Context/Get Boolean Value")]
    public class GetBooleanNode : SingleGraphValueGetterNode {
        [Output]
        [SerializeField]
        private NbtBooleanPort value = null;

        protected override string GetValuePortName() {
            return nameof(value);
        }
    }
}