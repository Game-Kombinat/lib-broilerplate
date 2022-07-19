using Broilerplate.Bt.Nodes.Data.Graph;
using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.ControlFlow.Bt.Data;
using GameKombinat.Fnbt;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Broilerplate.Editor.Broilerplate.Bt.Nodes {
    public abstract class SingleValueEditorBase : NodeEditor {
        private DataContextNameListProvider nameList;

        protected abstract bool ShowParentInputPort {
            get;
        }

        protected abstract string ParentPortName {
            get;
        }
        
        public override void OnBodyGUI() {
            serializedObject.Update();
            var node = (ContextDataAccessNode)target;
            if (nameList == null) {
                nameList = new DataContextNameListProvider(node);
            }

            var dataScope = (DataContextScope) serializedObject.FindProperty("dataContextScope").intValue;
            // ****************************************
            // Render Data Scope Selector
            EditorGUI.BeginChangeCheck();
            dataScope = (DataContextScope) EditorGUILayout.EnumPopup("Data Scope", dataScope);
            serializedObject.FindProperty("dataContextScope").intValue = (int) dataScope;
            
            if (EditorGUI.EndChangeCheck()) {
                nameList.SwitchScope(dataScope);
            }

            

            if (nameList.NameList.Length == 0) {
                EditorGUILayout.LabelField($"No variables in {node.DataContextScope} data context");
                if (GUILayout.Button("Add some")) {
                    DataContextEditorWindow.Show(dataScope, nameList.Data, nameList, window.position);
                }
                return;
            }
            
            // ****************************************
            // Check for variable renames
            nameList.UpdateNameList();
            // Check if the variable got renamed
            var currentName = serializedObject.FindProperty("varName");
            var currentIndex = serializedObject.FindProperty("nameListIndex");
            string oldName = currentName.stringValue;
            if (nameList.CheckForVariableRename(ref oldName, currentIndex.intValue)) {
                currentName.stringValue = oldName;
            }
            
            // ****************************************
            // Figure out if the port type is compatible with the variable type we have selected
            var inputPort = GetPort();
            bool matches;
            if (string.IsNullOrEmpty(currentName.stringValue)) {
                currentName.stringValue = nameList.NameList[0];
            } 

            var valueTag = nameList.Data.Find(currentName.stringValue)["value"];
            NbtTagType tagType = NbtPort.GetTagForType(inputPort.ValueType);
            if (valueTag != null) {
                tagType = nameList.Data.Find(currentName.stringValue)["value"].TagType;
            }
            matches = NbtPort.PortMatchesTagType(inputPort.ValueType, tagType);
            
            // ****************************************
            // Render Ports
            GUILayout.BeginVertical();
            {
                if (ShowParentInputPort) {
                    NodeEditorGUILayout.PortField(new GUIContent("Parent"), 
                        target.GetInputPort(ParentPortName), GUILayout.MinWidth(0));
                }
                if (matches) {
                    var displayTagType = nameList.Data.Find(currentName.stringValue)["value"].TagType;
                    // Because nbt doesn't know actual booleans and we use bytes for it,
                    // to keep the illusion up, we need this check and replace byte with boolean.
                    string display = displayTagType == NbtTagType.Byte ? "Boolean" : displayTagType.ToString();
                    EditorGUILayout.BeginHorizontal();
                    {
                        NodeEditorGUILayout.PortField(new GUIContent($"{display}"), inputPort, GUILayout.MinWidth(0));
                        if (node is SingleGraphValueSetterNode) {
                            NbtPort.DrawDefaultField(inputPort, node);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                }
                else {
                    EditorGUILayout.LabelField("Port/Var Type Mismatch");
                    // We have to disconnect from all the other nodes now, of course.
                    for (int i = 0; i < inputPort.ConnectionCount; ++i) {
                        inputPort.Disconnect(i);
                    }
                }
                
            }
            GUILayout.EndVertical();
            
            int index = nameList.GetIndex(currentName.stringValue);
            if (index < 0) {
                index = 0;
            }

            // ****************************************
            // Render Variable Selector Dropdown
            index = EditorGUILayout.Popup(index, nameList.NameList);
            currentName.stringValue = nameList.NameList[index];

            currentIndex.intValue = index;
            
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Edit Vars")) {
                // can use node reference here because properties have been applied and it's save
                DataContextEditorWindow.Show(node.DataContextScope, nameList.Data, nameList, window.position);
            }
            
        }

        public abstract NodePort GetPort();
    }
}