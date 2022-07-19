using System;
using Broilerplate.Bt.Nodes.Data.Graph;
using GameKombinat.Fnbt;
using UnityEditor;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
#endif

namespace Broilerplate.Bt.Nodes.Ports {
    [Serializable]
    public class NbtPort {
        public static bool PortMatchesTagType(Type portType, NbtTagType tagType) {
            // First check if we deal with a number, in that case any number type will do.
            if (typeof(NbtNumberPort).IsAssignableFrom(portType)) {
                switch (tagType) {
                    case NbtTagType.Int:
                    case NbtTagType.Float:
                        return true;
                }
            }
            // Then do boolean
            if (portType.IsAssignableFrom(typeof(NbtBooleanPort)) && tagType == NbtTagType.Byte) {
                return true;
            }
            
            // aaaand strings
            if (portType.IsAssignableFrom(typeof(NbtStringPort)) && tagType == NbtTagType.String) {
                return true;
            }

            return false;
        }

        public static NbtTagType GetTagForType(Type portType) {
            if (portType == typeof(NbtIntPort)) {
                return NbtTagType.Int;
            }

            if (portType == typeof(NbtFloatPort)) {
                return NbtTagType.Float;
            }

            if (portType == typeof(NbtBooleanPort)) {
                return NbtTagType.Byte;
            }

            if (portType == typeof(NbtStringPort)) {
                return NbtTagType.String;
            }
            throw new ArgumentOutOfRangeException($"{nameof(portType)} is of unsupported type: {portType}. No known NbtTagType exists for this.");
        }

        public static void DrawDefaultField(NodePort port, ContextDataAccessNode source) {
            #if UNITY_EDITOR
            var portType = port.ValueType;
            
            if (portType == typeof(NbtIntPort)) {
                var node = source as SetIntNode;
                if (node == null) {
                    EditorGUILayout.LabelField("Node type mismatch.");
                    return;
                }
                node.SetDefaultValue(EditorGUILayout.IntField(node.GetDefaultValue())); 
                return;
            }

            if (portType == typeof(NbtFloatPort)) {
                var node = source as SetFloatNode;
                if (node == null) {
                    EditorGUILayout.LabelField("Node type mismatch.");
                    return;
                }
                node.SetDefaultValue(EditorGUILayout.FloatField(node.GetDefaultValue())); 
                return;
            }

            if (portType == typeof(NbtBooleanPort)) {
                var node = source as SetBooleanNode;
                if (node == null) {
                    EditorGUILayout.LabelField("Node type mismatch.");
                    return;
                }
                node.SetDefaultValue(EditorGUILayout.Toggle(node.GetDefaultValue())); 
                return;
            }

            if (portType == typeof(NbtStringPort)) {
                var node = source as SetStringNode;
                if (node == null) {
                    EditorGUILayout.LabelField("Node type mismatch.");
                    return;
                }
                node.SetDefaultValue(EditorGUILayout.TextField(node.GetDefaultValue(), GUILayout.Width(100))); 
                return;
            }
            EditorGUILayout.LabelField("Unknown port value type.");
            #endif
        }
    }
}