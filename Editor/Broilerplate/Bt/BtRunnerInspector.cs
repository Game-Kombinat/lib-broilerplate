using Broilerplate.Bt.Nodes;
using GameKombinat.ControlFlow.Bt;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Broilerplate.Editor.Broilerplate.Bt {
    [CustomEditor(typeof(UnityGraphRunner))]
    public class BtRunnerInspector : UnityEditor.Editor {
        private DataContextEditorDrawer contextDrawer;
        
        private void OnEnable() {
            contextDrawer = new DataContextEditorDrawer();
            var runner = (UnityGraphRunner)target;
            if (runner.runnable != null) {
                runner.runnable.Prepare();
                contextDrawer.Prepare(runner.runnable.Data);
            }
        }
        public override void OnInspectorGUI() {
            var runner = (UnityGraphRunner)target;
            Undo.RecordObject(target, "Editing BtRunner");
            EditorGUI.BeginChangeCheck();
            runner.beginOnStart = EditorGUILayout.Toggle("Begin On Start", runner.beginOnStart);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (GUILayout.Button("Edit Graph")) {
                    if (runner.runnable == null) {
                        runner.runnable = CreateInstance<BehaviourTree>();
                        runner.runnable.name = target.name;
                        runner.runnable.Prepare();
                        contextDrawer.Prepare(runner.runnable.Data);
                        var root = runner.runnable.AddNode(typeof(RootNode));
                        root.name = "Root Node";
                    }

                    NodeGraphEditor.GetEditor(runner.runnable, NodeEditorWindow.Open(runner.runnable));
                }
                
                if (GUILayout.Button("Make Graph Unique")) {
                    if (runner.runnable != null) {
                        runner.runnable = (BehaviourTree)runner.runnable.Copy();
                    }
                }
            
                if (GUILayout.Button("Clear Graph")) {
                
                    if (EditorUtility.DisplayDialog("Really Clear Graph?", "This will delete the whole Behaviour Tree and it cannot be undone. Sure?", "Yes", "No!!!")) {
                        runner.runnable = null;
                    }
                }
            }
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Graph Data Context", EditorStyles.largeLabel);
            contextDrawer.Draw();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
            }
            
        }
    }
}