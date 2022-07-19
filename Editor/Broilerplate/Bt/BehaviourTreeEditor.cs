using System.Collections.Generic;
using Broilerplate.Bt;
using Broilerplate.Bt.Nodes.Ports;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Broilerplate.Editor.Broilerplate.Bt {
    /// <summary>
    /// This basically only exists because I wish to define custom noodle colours
    /// </summary>
    [CustomNodeGraphEditor(typeof(BehaviourTree), "BehaviourTree.Settings")]
    public class BehaviourTreeEditor : NodeGraphEditor {
        public override NodeEditorPreferences.Settings GetDefaultPreferences() {
            return new NodeEditorPreferences.Settings() {
                typeColors = new Dictionary<string, Color>() {
                    // base types
                    { typeof(Port).PrettyName(), new Color32(153, 178, 134, 255) },
                    { typeof(NbtPort).PrettyName(), new Color32(178, 139, 79, 255) },
                    { typeof(NbtNumberPort).PrettyName(), new Color32(206, 164, 34, 255) },
                    
                    // concrete port types
                    { typeof(NbtBooleanPort).PrettyName(), new Color32(39, 163, 172, 255) },
                    { typeof(NbtIntPort).PrettyName(), new Color32(49, 172, 102, 255) },
                    { typeof(NbtFloatPort).PrettyName(), new Color32(65, 105, 172, 255) },
                    { typeof(NbtStringPort).PrettyName(), new Color32(172, 50, 77, 255) },
                },
            };
        }

        public override void OnGUI() {
            var tree = (BehaviourTree)target;
            EditorGUILayout.BeginHorizontal();
            {
                if (tree.HasParent) {
                    if (GUILayout.Button($"Back to Parent({tree.Parent.name})")) {
                        NodeGraphEditor.GetEditor(tree.Parent, NodeEditorWindow.Open(tree.Parent));
                    }

                    if (!tree.Parent.IsRoot) { // need to check on the parent. But Root will be the same for both in all cases
                        var root = tree.Root;
                        if (GUILayout.Button($"Back to Root({root.name})")) {
                            NodeGraphEditor.GetEditor(root, NodeEditorWindow.Open(root));
                        }
                    }
                    
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}