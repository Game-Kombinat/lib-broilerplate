using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.FloatPort)]
    [CreateNodeMenu("Data Context/Get Float Value")]
    public class GetFloatNode : SingleGraphValueGetterNode {
        [Output]
        private NbtFloatPort value = null;

        protected override string GetValuePortName() {
            return nameof(value);
        }
    }
}