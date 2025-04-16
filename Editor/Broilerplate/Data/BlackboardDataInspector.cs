using Broilerplate.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Broilerplate.Editor.Broilerplate.Data {
    
    [CustomEditor(typeof(BlackboardData))]
    public class BlackboardDataInspector : UnityEditor.Editor {
        private ReorderableList entryList;

        private void OnEnable() {
            entryList = new ReorderableList(serializedObject, serializedObject.FindProperty("entries"), true, true, true, true) {
                drawHeaderCallback = rect => {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * .3f, EditorGUIUtility.singleLineHeight), "Key");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * .3f + 10, rect.y, rect.width * .3f, EditorGUIUtility.singleLineHeight), "Type");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * .6f + 5, rect.y, rect.width * .4f, EditorGUIUtility.singleLineHeight), "Value");
                },
                
                drawElementCallback = (rect, index, active, focused) => {
                    var element = entryList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;

                    var keyName = element.FindPropertyRelative("keyName");
                    var valueType = element.FindPropertyRelative("valueType");
                    var value = element.FindPropertyRelative("value");
                    
                    var keyRect = new Rect(rect.x, rect.y, rect.width * .3f, EditorGUIUtility.singleLineHeight);
                    var valueTypeRect = new Rect(rect.x + rect.width * .3f, rect.y, rect.width * .3f, EditorGUIUtility.singleLineHeight);
                    var valueRect = new Rect(rect.x + rect.width * .6f, rect.y, rect.width * .4f, EditorGUIUtility.singleLineHeight);

                    EditorGUI.PropertyField(keyRect, keyName, GUIContent.none);
                    EditorGUI.PropertyField(valueTypeRect, valueType, GUIContent.none);

                    switch ((AnyValue.ValueType)valueType.enumValueIndex) {
                        case AnyValue.ValueType.Int:
                            var intValue = value.FindPropertyRelative("intValue");
                            EditorGUI.PropertyField(valueRect, intValue, GUIContent.none);
                            break;
                        case AnyValue.ValueType.Float:
                            var floatValue = value.FindPropertyRelative("floatValue");
                            EditorGUI.PropertyField(valueRect, floatValue, GUIContent.none);
                            break;
                        case AnyValue.ValueType.Vector3:
                            var vec3Value = value.FindPropertyRelative("vector3Value");
                            EditorGUI.PropertyField(valueRect, vec3Value, GUIContent.none);
                            break;
                    }
                }
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            entryList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}