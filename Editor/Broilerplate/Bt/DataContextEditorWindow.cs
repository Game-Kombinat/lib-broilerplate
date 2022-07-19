using GameKombinat.ControlFlow.Bt.Data;
using UnityEditor;
using UnityEngine;

namespace Broilerplate.Editor.Broilerplate.Bt {
    public class DataContextEditorWindow : EditorWindow {
        public static void Show(DataContextScope scope, DataContext data, DataContextNameListProvider nameProvider, Rect nodeWindowPosition) {
            var window = CreateInstance<DataContextEditorWindow>();// GetWindow<DataContextEditorWindow>();
            window.Prepare(data, nameProvider);
            window.titleContent = new GUIContent($"{scope} Data Context Ed");
            window.ShowUtility();
            nodeWindowPosition.width = 350;
            nodeWindowPosition.height -= 50;
            nodeWindowPosition.x += 15;
            nodeWindowPosition.y += 65;
            window.position = nodeWindowPosition;
        }

        private DataContextNameListProvider nameProvider;
        private DataContextEditorDrawer drawer;

        public void Prepare(DataContext data, DataContextNameListProvider nameProvider) {
            this.nameProvider = nameProvider;
            drawer = new DataContextEditorDrawer();
            drawer.Prepare(data);
        }

        private void OnGUI() {
            EditorGUILayout.BeginVertical();
            {
                if (drawer.Draw()) {
                    nameProvider.UpdateNameList(true);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void OnLostFocus() {
            nameProvider.UpdateNameList();
            Close();
        }
    }
}