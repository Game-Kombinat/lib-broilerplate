using Broilerplate.Bt.Nodes.Data.Graph;
using XNode;

namespace Broilerplate.Editor.Broilerplate.Bt.Nodes {
    [CustomNodeEditor(typeof(SingleGraphValueGetterNode))]
    public class SingleGraphValueGetterNodeEditor : SingleValueEditorBase {
        protected override bool ShowParentInputPort => false;
        protected override string ParentPortName => string.Empty;
        public override NodePort GetPort() {
            return target.GetOutputPort("value");
        }
    }
}