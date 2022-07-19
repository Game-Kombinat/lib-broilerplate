using Broilerplate.Bt.Nodes;
using GameKombinat.ControlFlow.Bt;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Broilerplate.Editor.Broilerplate.Bt.Nodes {
    [CustomNodeEditor(typeof(SubTreeNode))]
    public class SubTreeNodeEditor : NodeEditor {
        public override void OnBodyGUI() {
            var node = (SubTreeNode)target;
            EditorGUILayout.BeginVertical();
            {
                NodeEditorGUILayout.PortField(new GUIContent("Parent"), 
                    target.GetInputPort("parent"), GUILayout.MinWidth(0));
                if (GUILayout.Button("Edit Graph")) {
                    if (node.subTree == null) {
                        node.subTree = ScriptableObject.CreateInstance<BehaviourTree>();
                        node.subTree.name = node.name + " SubTree";
                        node.subTree.SetParent(node.Tree);
                        node.subTree.Prepare();
                        var root = node.subTree.AddNode(typeof(RootNode));
                        root.name = "Root Node";
                    }

                    NodeGraphEditor.GetEditor(node.subTree, NodeEditorWindow.Open(node.subTree));
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}