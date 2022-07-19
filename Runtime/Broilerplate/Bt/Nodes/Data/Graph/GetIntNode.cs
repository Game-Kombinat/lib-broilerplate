using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    [NodeTint(NodeColors.IntPort)]
    [CreateNodeMenu("Data Context/Get Integer Value")]
    public class GetIntNode : SingleGraphValueGetterNode {
        [Output]
        private NbtIntPort value = null;
        
        protected override string GetValuePortName() {
            return nameof(value);
        }
    }
}