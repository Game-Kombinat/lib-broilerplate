using GameKombinat.ControlFlow.Bt.Data;
using UnityEditor;

namespace Broilerplate.Editor.Broilerplate.Bt {
    [CustomEditor(typeof(DataContext))]
    public class DataContextInspector : UnityEditor.Editor {
        private DataContextEditorDrawer contextDrawer;
        private void OnEnable() {
            contextDrawer = new DataContextEditorDrawer();
            contextDrawer.Prepare(target as DataContext);
        }

        public override void OnInspectorGUI() {
            contextDrawer.Draw();
        }
    }
}