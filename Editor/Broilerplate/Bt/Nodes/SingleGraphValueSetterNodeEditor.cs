using Broilerplate.Bt.Nodes.Data.Graph;
using XNode;

namespace Broilerplate.Editor.Broilerplate.Bt.Nodes {
    [CustomNodeEditor(typeof(SingleGraphValueSetterNode))]
    public class SingleGraphValueSetterNodeEditor : SingleValueEditorBase {
        protected override bool ShowParentInputPort => true;
        protected override string ParentPortName => "parent";
        public override NodePort GetPort() {
            return target.GetInputPort("inputValue");
        }
    }
}