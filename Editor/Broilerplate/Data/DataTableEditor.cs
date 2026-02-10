using System.Collections.Generic;
using System.Linq;
using Broilerplate.Data;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Broilerplate.Editor.Broilerplate.Data {
    public enum ClickAction {
        Nothing,
        Edit,
        Delete
    }
    public class DataTableEditor : EditorWindow {
//         #region static stuff
//
//         // Oh lord ...
//         [OnOpenAsset]
// #if UNITY_2021_3_OR_NEWER
//         public static bool OpenEditor(int instanceID) {
// #else
//         public static bool OpenEditor(int instanceID, int line) {
// #endif
//         
//         
//             var obj = EditorUtility.InstanceIDToObject(instanceID) as IDataTable;
//             if (obj == null) {
//                 return false;
//             }
//
//
//             var openEditors = Resources.FindObjectsOfTypeAll<DataTableEditor>();
//             for (int i = 0; i < openEditors.Length; i++) {
//                 var ed = openEditors[i];
//                 if (ed.iDataTable == obj) {
//                     ed.Prepare(obj as ScriptableObject);
//                     ed.Show();
//                     ed.Focus();
//                     return true;
//                 }
//             }
//             var window = CreateWindow<DataTableEditor>("Data Table Editor");
//
//             window.Prepare(obj as ScriptableObject);
//             window.Show();
//             window.Focus();
//             return true;
//         }
//
//         #endregion


        private IDataTable iDataTable;
        private ScriptableObject unityDataTable;

        private SerializedObject serializedObject;

        // because unitys SerializedObject API is theoretically good
        // but practically crap. (FindRelativeProperty still broken. Guys ...)
        private List<SerializedObject> unityRowData;

        private Vector2 scrollPosition;
        
        private string lastSearchString;
        private string currentSearchString;

        private void Prepare(ScriptableObject asset) {
            iDataTable = (IDataTable)asset;
            iDataTable.Reset();
            unityDataTable = asset;
            serializedObject = new SerializedObject(unityDataTable);
            unityRowData = new List<SerializedObject>();
            ValidateUnityRowData(true);
        }

        private void ValidateUnityRowData(bool force = false) {
            if (force || lastSearchString != currentSearchString) {
                var sourceList = iDataTable.GetRows();
                unityRowData.Clear();
                string search = currentSearchString?.ToLower();
                unityRowData.AddRange(sourceList.Where(x => x.Search(search)).Select(x => new SerializedObject(x)));
                lastSearchString = currentSearchString;
            }
        }

        private void OnGUI() {
            if (iDataTable == null) {
                EditorGUILayout.LabelField("Double-Click on a DataTable to edit it");
                return;
            }

            if (GUILayout.Button("Add Row")) {
                InsertNewRow();
            }

            currentSearchString = EditorGUILayout.TextField("Search", currentSearchString);

            ValidateUnityRowData();
            var columnInfos = iDataTable.GetColumnInfo();
            // 80 units width is for extra stuff. that is:
            // 1: vertical scroll bar (24)
            // 2: edit and delete buttons in front (48 for both)
            // 3: Some extra margin for good measure
            float columnWidth = Mathf.Max(200, (position.width - 80) / Mathf.Max(1, columnInfos.Count));

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical();

            DrawHeader(columnInfos, columnWidth);
            EditorGUILayout.Space();
            DrawRows(columnInfos, columnWidth);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void InsertNewRow() {
            Undo.RecordObject(unityDataTable, "New Row");
            iDataTable.AddRow();
            EditorUtility.SetDirty(unityDataTable);
            serializedObject.Update();
            ValidateUnityRowData(true);
        }

        private static void DrawHeader(List<ColumnDescriptor> columnInfos, float columnWidth) {
            GUI.enabled = true;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X", EditorStyles.boldLabel, GUILayout.Width(24));
            for (var i = 0; i < columnInfos.Count; i++) {
                var columnInfo = columnInfos[i];
                float targetColWidth = columnInfo.propertyName == "id" ? 50 : columnWidth;
                EditorGUILayout.LabelField(columnInfo.displayName, EditorStyles.boldLabel, GUILayout.Width(targetColWidth));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRows(List<ColumnDescriptor> columnInfos, float columnWidth) {
            int index = -1;
            ClickAction action = ClickAction.Nothing;
            for (var i = 0; i < unityRowData.Count; i++) {
                action = DrawRow(columnInfos, unityRowData[i], columnWidth);
                if (action != ClickAction.Nothing) {
                    index = i;
                    break;
                }
            }

            switch (action) {
                case ClickAction.Edit:
                    Debug.Log("Edit this!");
                    break;
                case ClickAction.Delete:
                    DeleteRow(index);
                    break;
                case ClickAction.Nothing:
                default:
                    break;
            }
            GUI.enabled = true;
        }

        private void DeleteRow(int index) {
            Undo.RecordObject(unityDataTable, "Delete row");
            var asset = unityRowData[index];
            
            iDataTable.RemoveRow(asset.targetObject as RowData);
            EditorUtility.SetDirty(unityDataTable);
            ValidateUnityRowData(true);
        }

        private static ClickAction DrawRow(List<ColumnDescriptor> columnInfos, SerializedObject row, float columnWidth) {
            
            var hiddenLabel = new GUIContent();
            var action = ClickAction.Nothing;
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("X", GUILayout.Width(24))) {
                action = ClickAction.Delete;
            }
            
            Undo.RecordObject(row.targetObject, $"Edit {row.targetObject.name}");
            for (var j = 0; j < columnInfos.Count; j++) {
                var columnInfo = columnInfos[j];
                GUI.enabled = !columnInfo.readOnly;
                // this is a fixed column and I don't want it to be that long.
                float targetColWidth = columnInfo.propertyName == "id" ? 50 : columnWidth;
                var prop = row.FindProperty(columnInfo.propertyName);
                if (prop != null) {
                    EditorGUILayout.PropertyField(row.FindProperty(columnInfo.propertyName), hiddenLabel, GUILayout.Width(targetColWidth));
                }
                else {
                    EditorGUILayout.TextField("", columnInfo.field.GetValue(row.targetObject).ToString());
                    
                }
            }

            row.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            return action;
        }
    }
}