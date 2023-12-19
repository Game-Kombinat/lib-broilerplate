using System;
using System.Collections.Generic;
using System.Linq;
using Broilerplate.Bt.Data;
using Broilerplate.Data;
using GameKombinat.Fnbt;
using UnityEditor;
using UnityEngine;

namespace Broilerplate.Editor.Broilerplate.Bt {
    /// <summary>
    /// Helper class to draw the data context object with a search field and stuff.
    /// </summary>
    public class DataContextEditorDrawer {

        public enum EditorTagType {
            Integer,
            Float,
            Bool,
            String
        }
        private DataContext data;
        private List<NbtCompound> filterList;
        private NbtCompound newFieldTemplate;
        private NbtCompound tagForDeletion;

        private string searchText;

        /// <summary>
        /// This is set to true in DrawSingleTag, when the tag type was changed or a tag was deleted.
        /// </summary>
        private bool filterListNeedsRebuild;
        
        public void Prepare(DataContext data) {
            this.data = data;
            newFieldTemplate = new NbtCompound();
            newFieldTemplate["tagName"] = new NbtString("tagName", "New Field Name");
            newFieldTemplate["value"] = new NbtString("value", "Default Value");
            searchText = null;
            tagForDeletion = null;
            filterList = null;
            filterListNeedsRebuild = false;
        }
        
        public bool Draw() {
            if (data == null) {
                return false;
            }
            Undo.RecordObject(data, "Edit Data Context");
            EditorGUI.BeginChangeCheck();
            DrawSearchBar();
            DrawTagList();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(data);
                return true;
            }

            return false;
        }

        private void DrawSearchBar() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Search: ");
                searchText = EditorGUILayout.TextField(searchText);
                if (filterList == null || filterListNeedsRebuild) {
                    filterList = data.DataList.OfType<NbtCompound>().ToList();
                }
                
                if (EditorGUI.EndChangeCheck()) {
                    if (!string.IsNullOrEmpty(searchText)) {
                        var lowerSearch = searchText.ToLower();
                        filterList = data.DataList.OfType<NbtCompound>().Where(x => x["tagName"].StringValue.ToLower().Contains(lowerSearch)).ToList();
                    }
                    else {
                        filterList = data.DataList.OfType<NbtCompound>().ToList();
                    }
                }

                filterListNeedsRebuild = false;
            }
            EditorGUILayout.EndHorizontal();
        }

        private Vector2 tagListScroll;
        private void DrawTagList() {
            DrawAddField();
            tagListScroll = EditorGUILayout.BeginScrollView(tagListScroll, GUILayout.MaxHeight(350));
            {
                EditorGUILayout.BeginVertical();
                {
                    for (int i = 0; i < filterList.Count; ++i) {
                        DrawSingleTag(filterList[i]);
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            
            if (tagForDeletion != null) {
                data.DataList.Remove(tagForDeletion);
                tagForDeletion = null;
                filterListNeedsRebuild = true;
            }
            
        }

        
        private void DrawAddField() {
            DrawSingleTag(newFieldTemplate, false);
            if (data.HasAnyTagByName(newFieldTemplate["tagName"].StringValue)) {
                EditorGUILayout.LabelField("Name already exists");
            }
            else {
                if (GUILayout.Button("Add this")) {
                
                    data.DataList.Add((NbtTag)newFieldTemplate.Clone());
                    newFieldTemplate["tagName"] = new NbtString("tagName", string.Empty);
                    filterListNeedsRebuild = true;
                }
            }
            EditorGUILayout.Space();
            
        }

        private void DrawSingleTag(NbtCompound tag, bool drawDeleteButton = true) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                string oldTagName = tag["tagName"].StringValue;
                // *** Draw Name
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Name");
                    var nameTag = (NbtString) tag["tagName"];
                    
                    string newName = EditorGUILayout.TextField(oldTagName);
                    
                    // We could just not set this, it will actually work good. But it will not be apparent why it's not working
                    nameTag.Value = newName;
                    if (drawDeleteButton) {
                        if (GUILayout.Button("X", GUILayout.Width(25))) {
                            tagForDeletion = tag;
                        }
                    }
                    
                }
                EditorGUILayout.EndHorizontal();
                
                // Check for duplicates of the new tag name
                var existing = data.Find(tag["tagName"].StringValue);
                if (existing != null && existing != tag) {
                    Debug.LogError($"The name {tag["tagName"].StringValue} already exists in this data context. Duplicate names are not allowed, use another one!");
                    var nameTag = (NbtString) tag["tagName"];
                    nameTag.Value = oldTagName;
                }
                
                // *** Draw tag Type Selection
                EditorGUILayout.BeginHorizontal();
                {
                    var valueTag = tag["value"];
                    var tagType = NbtToEditorTag(valueTag.TagType);
                    EditorGUILayout.LabelField("Type");
                    EditorGUI.BeginChangeCheck();
                    tagType = (EditorTagType)EditorGUILayout.EnumPopup(tagType);
                    if (EditorGUI.EndChangeCheck()) {
                        tag["value"] = EditorTagTypeToNbtTag(tagType);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // *** Draw Tag Value
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Value");
                    DrawValueField(tag);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawValueField(NbtCompound tag) {
            EditorGUI.BeginChangeCheck();
            NbtTag valueTag = tag["value"];
            if (valueTag == null) {
                EditorGUILayout.LabelField($"No value tag exists for variable {tag["tagName"].StringValue}. That's a bug for sure.");
                return;
            }
            switch (valueTag.TagType) {
                case NbtTagType.Byte:
                    var byteTag = (NbtByte) valueTag;
                    bool isOn = byteTag.Value == 1;
                    byteTag.Value = (byte)(EditorGUILayout.Toggle(isOn) ? 1 : 0);
                    break;
                case NbtTagType.Int:
                    var intTag = (NbtInt) valueTag;
                    intTag.Value = EditorGUILayout.IntField(intTag.Value);
                    break;
                case NbtTagType.Float:
                    var floatTag = (NbtFloat) valueTag;
                    floatTag.Value = EditorGUILayout.FloatField(floatTag.Value);
                    break;
                case NbtTagType.String:
                    var stringTag = (NbtString) valueTag;
                    stringTag.Value = EditorGUILayout.TextField(stringTag.Value);
                    break;
                default:
                    EditorGUILayout.LabelField($"Tag Type {tag.TagType} is not supported.");
                    break;
            }

            if (EditorGUI.EndChangeCheck()) {
                filterListNeedsRebuild = true;
            }
            
        }

        private EditorTagType NbtToEditorTag(NbtTagType tagType) {
            switch (tagType) {
                
                // Supported cases
                case NbtTagType.Int:
                    return EditorTagType.Integer;
                case NbtTagType.Float:
                    return EditorTagType.Float;
                case NbtTagType.String:
                    return EditorTagType.String;
                case NbtTagType.Byte:
                    return EditorTagType.Bool;
                default:
                    // Yeah not happening but you know. We need a default case anyway.
                    throw new ArgumentOutOfRangeException(nameof(tagType), tagType, "This tag type is not accounted for!");
            }
        }

        private NbtTag EditorTagTypeToNbtTag(EditorTagType editorTagType) {
            switch (editorTagType) {
                case EditorTagType.Integer:
                    return new NbtInt("value", 0);
                case EditorTagType.Float:
                    
                    return new NbtFloat("value", 0);
                case EditorTagType.Bool: 
                    return new NbtByte("value", 0x00);
                case EditorTagType.String:
                    return new NbtString("value", string.Empty);
                default:
                    throw new ArgumentOutOfRangeException(nameof(editorTagType), editorTagType, "Given editor tag type cannot be converted to an nbt tag type.");
            }
        }
    }
}
