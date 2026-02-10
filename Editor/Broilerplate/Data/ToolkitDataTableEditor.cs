using System.Collections.Generic;
using System.Linq;
using Broilerplate.Data;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Broilerplate.Editor.Broilerplate.Data {
    public class ToolkitDataTableEditor : EditorWindow {
        #region static stuff

        [OnOpenAsset]
        #if UNITY_2021_3_OR_NEWER
        public static bool OpenEditor(int instanceID) {
            #else
        public static bool OpenEditor(int instanceID, int line) {
            #endif
            var obj = EditorUtility.InstanceIDToObject(instanceID) as IDataTable;
            if (obj == null) {
                return false;
            }

            var openEditors = Resources.FindObjectsOfTypeAll<ToolkitDataTableEditor>();
            for (int i = 0; i < openEditors.Length; i++) {
                var ed = openEditors[i];
                if (ed.iDataTable == obj) {
                    ed.Prepare(obj as ScriptableObject);
                    ed.Show();
                    ed.Focus();
                    return true;
                }
            }

            var window = CreateWindow<ToolkitDataTableEditor>("Data Table Editor");

            window.Prepare(obj as ScriptableObject);
            window.Show();
            window.Focus();
            return true;
        }

        #endregion

        private IDataTable iDataTable;
        private ScriptableObject unityDataTable;
        private SerializedObject serializedObject;
        private List<SerializedObject> unityRowData;

        private string lastSearchString;
        private string currentSearchString;

        private VisualElement rootContainer;
        private ScrollView tableScrollView;
        private VisualElement tableHeader;
        private VisualElement tableBody;
        private TextField searchField;
        private Button addRowButton;

        private void Prepare(ScriptableObject asset) {
            iDataTable = (IDataTable)asset;
            iDataTable.Reset();
            unityDataTable = asset;
            serializedObject = new SerializedObject(unityDataTable);
            unityRowData = new List<SerializedObject>();
            ValidateUnityRowData(true);
            RebuildUI();
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

        private void CreateGUI() {
            rootContainer = rootVisualElement;

            if (iDataTable == null) {
                var label = new Label("Double-Click on a DataTable to edit it");
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.marginTop = 20;
                rootContainer.Add(label);
                return;
            }

            RebuildUI();
        }

        private void RebuildUI() {
            if (rootContainer == null) {
                return;
            }

            rootContainer.Clear();

            // Toolbar
            var toolbar = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5
                }
            };

            addRowButton = new Button(InsertNewRow);
            addRowButton.text = "Add Row";
            addRowButton.style.marginRight = 10;
            toolbar.Add(addRowButton);

            searchField = new TextField("Search");
            searchField.style.flexGrow = 1;
            searchField.RegisterValueChangedCallback(evt => {
                currentSearchString = evt.newValue;
                ValidateUnityRowData();
                RebuildTableBody();
            });
            toolbar.Add(searchField);

            rootContainer.Add(toolbar);

            // Table container
            tableScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            tableScrollView.style.flexGrow = 1;

            var tableContainer = new VisualElement {
                style = {
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5
                }
            };

            // Header
            tableHeader = new VisualElement();
            BuildTableHeader();
            rootContainer.Add(tableHeader);

            // Body
            tableBody = new VisualElement();
            RebuildTableBody();
            tableContainer.Add(tableBody);

            tableScrollView.Add(tableContainer);
            rootContainer.Add(tableScrollView);
        }

        private void BuildTableHeader() {
            tableHeader.Clear();

            var columnInfos = iDataTable.GetColumnInfo();
            var headerRow = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    borderBottomWidth = 2,
                    borderBottomColor = new Color(0.2f, 0.2f, 0.2f),
                    paddingBottom = 5,
                    marginBottom = 5,
                    marginRight = 5,
                }
            };

            // Delete button column
            var deleteHeader = new Label("X") {
                style = {
                    width = 30,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            headerRow.Add(deleteHeader);

            foreach (var columnInfo in columnInfos) {
                var headerLabel = new Label(columnInfo.displayName) {
                    style = {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        flexGrow = 1,
                        minWidth = 0,
                    }
                };
                
                headerRow.Add(headerLabel);
            }

            tableHeader.Add(headerRow);
        }

        private void RebuildTableBody() {
            if (tableBody == null) {
                return;
            }

            tableBody.Clear();

            var columnInfos = iDataTable.GetColumnInfo();

            for (int i = 0; i < unityRowData.Count; i++) {
                var rowIndex = i;
                var rowData = unityRowData[i];
                var rowElement = CreateRowElement(rowData, columnInfos, rowIndex);
                tableBody.Add(rowElement);
            }
        }

        private VisualElement CreateRowElement(SerializedObject row, List<ColumnDescriptor> columnInfos, int rowIndex) {
            var rowElement = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 5,
                    marginRight = 5,
                    paddingBottom = 5,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.3f, 0.3f, 0.3f)
                }
            };

            // Delete button
            var deleteButton = new Button(() => DeleteRow(rowIndex)) {
                text = "X",
                style = {
                    width = 30,
                }
            };
            rowElement.Add(deleteButton);

            foreach (var columnInfo in columnInfos) {
                var prop = row.FindProperty(columnInfo.propertyName);

                VisualElement fieldElement;

                if (prop != null) {
                    Debug.Log($"{prop.name} is type {prop.propertyType}");
                    if (SerializablePropertyField.IsComplexOrReferenceType(prop)) {
                        var propertyField = new SerializablePropertyField(prop, "") {
                            style = {
                                flexGrow = 1,
                                minWidth = 0,
                            }
                        };
                        propertyField.SetEnabled(!columnInfo.readOnly);
                        propertyField.Bind(row);
                        fieldElement = propertyField;
                    }
                    else {
                        var propertyField = new PropertyField(prop, "") {
                            style = {
                                flexGrow = 1,
                                minWidth = 0,
                            }
                        };
                        propertyField.SetEnabled(!columnInfo.readOnly);
                        propertyField.Bind(row);
                        fieldElement = propertyField;
                    }
                    
                }
                else {
                    var textField = new TextField("") {
                        value = columnInfo.field.GetValue(row.targetObject)?.ToString() ?? "",
                        style = {
                            flexGrow = 1,
                            minWidth = 0,
                        }
                    };
                    textField.SetEnabled(false);
                    fieldElement = textField;
                }
                rowElement.Add(fieldElement);
            }

            return rowElement;
        }

        private void InsertNewRow() {
            Undo.RecordObject(unityDataTable, "New Row");
            iDataTable.AddRow();
            EditorUtility.SetDirty(unityDataTable);
            serializedObject.Update();
            ValidateUnityRowData(true);
            RebuildTableBody();
        }

        private void DeleteRow(int index) {
            Undo.RecordObject(unityDataTable, "Delete row");
            var asset = unityRowData[index];

            iDataTable.RemoveRow(asset.targetObject as RowData);
            EditorUtility.SetDirty(unityDataTable);
            ValidateUnityRowData(true);
            RebuildTableBody();
        }
    }
}