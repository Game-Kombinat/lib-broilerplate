using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.StringPort)]
    [CreateNodeMenu("Data Context/Get String Value")]
    public class GetStringNode : SingleGraphValueGetterNode {
        [Output]
        private NbtStringPort value = null;

        protected override string GetValuePortName() {
            return nameof(value);
        }
    }
}