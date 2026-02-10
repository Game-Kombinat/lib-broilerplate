using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Broilerplate.Editor.Broilerplate.Data {
    /// <summary>
    /// Reflects a visual element that can handle managed references and generic serializable types.
    /// This is a layout-stable variant for the imguicontainer implementation that unity provides.
    ///
    /// It can also understand and render nested complex types.
    /// </summary>
    public class SerializablePropertyField : VisualElement {
        public SerializablePropertyField(SerializedProperty property, string label = null) {
            var displayLabel = string.IsNullOrEmpty(label) ? property.displayName : label;

            var container = new VisualElement();
            Add(container);

            // Check if this is a managed reference (SerializeReference) and render a selector on top
            if (IsReferenceType(property)) {
                BuildManagedReferenceUI(property, container, displayLabel);
            }
            else if (IsComplexType(property)) {
                BuildComplexTypeUI(property, container);
            }
            else {
                BuildPrimitiveUI(container, property, displayLabel);
            }
        }

        private VisualElement CreateFoldout(string displayLabel) {
            return new Foldout {
                text = displayLabel,
                value = false
            };
        }

        private void BuildManagedReferenceUI(SerializedProperty prop, VisualElement container, string displayLabel) {
            var foldout = CreateFoldout(displayLabel);
            container.Add(foldout);
            var selector = new SubclassSelectorElement(prop);
            foldout.Add(selector);

            var propsContainer = new VisualElement {
                style = {
                    marginLeft = 15,
                    marginTop = 2,
                    marginBottom = 2
                }
            };
            foldout.Add(propsContainer);
            selector.OnTypeChanged += (_, property) => {
                if (property.managedReferenceValue != null) {
                    BuildNestedProperties(propsContainer, property);
                }
                else {
                    propsContainer.Clear();
                }
            };

            if (prop.managedReferenceValue != null) {
                BuildNestedProperties(propsContainer, prop);
            }
        }

        private void BuildPrimitiveUI(VisualElement propsContainer, SerializedProperty prop, string label) {
            // Because unity is apparently incapable of auto-detecting flags enums. Rarely seen such mickey mouse code fumblings.
            var propertyField = BuildPrimitiveField(prop, label);
            propsContainer.Add(propertyField);
        }

        private static VisualElement BuildPrimitiveField(SerializedProperty prop, string label) {
            VisualElement propertyField;
            if (IsBitField(prop, out var type)) {
                var flagsField = new KombinatEnumField(prop, (Enum)Enum.ToObject(type, prop.boxedValue), label);
                propertyField = flagsField;
            }
            else {
                var actualField = new PropertyField(prop, label);
                actualField.BindProperty(prop);
                propertyField = actualField;
            }

            return propertyField;
        }

        private void BuildComplexTypeUI(SerializedProperty prop, VisualElement container) {
            var propsContainer = new VisualElement {
                style = {
                    marginLeft = 15,
                    marginTop = 2,
                    marginBottom = 2
                }
            };
            container.Add(propsContainer);
            BuildNestedProperties(propsContainer, prop);
        }

        private void BuildNestedProperties(VisualElement propsContainer, SerializedProperty prop) {
            propsContainer.Clear();

            if (prop.isArray && prop.propertyType != SerializedPropertyType.String) {
                BuildArrayUI(propsContainer, prop);
            }
            else {
                BuildObjectUI(propsContainer, prop);
            }
        }

        private void BuildArrayUI(VisualElement propsContainer, SerializedProperty prop) {
            // Array size field
            var sizeProperty = prop.FindPropertyRelative("Array.size");
            if (sizeProperty != null) {
                var sizeField = new PropertyField(sizeProperty);
                sizeField.BindProperty(sizeProperty);
                sizeField.RegisterValueChangeCallback(evt => {
                    prop.serializedObject.ApplyModifiedProperties();
                    prop.serializedObject.Update();
                    EditorApplication.delayCall += () => BuildNestedProperties(propsContainer, prop);
                });
                propsContainer.Add(sizeField);
            }

            // Array elements
            int arraySize = prop.arraySize;
            for (int i = 0; i < arraySize; i++) {
                var element = prop.GetArrayElementAtIndex(i);
                var elementField = new SerializablePropertyField(element, $"Element {i}");
                propsContainer.Add(elementField);
            }
        }

        private void BuildObjectUI(VisualElement propsContainer, SerializedProperty prop) {
            var iterator = prop.Copy();
            var endProperty = iterator.GetEndProperty();

            if (!iterator.NextVisible(true)) {
                return;
            }

            do {
                if (SerializedProperty.EqualContents(iterator, endProperty)) {
                    break;
                }

                var childProperty = iterator.Copy();
                var childField = new SerializablePropertyField(childProperty);
                propsContainer.Add(childField);
            } while (iterator.NextVisible(false));
        }
        
        public static bool IsComplexOrReferenceType(SerializedProperty prop) {
            return prop.propertyType is SerializedPropertyType.Generic or SerializedPropertyType.ManagedReference;
        }

        private static bool IsComplexType(SerializedProperty prop) {
            return prop.propertyType is SerializedPropertyType.Generic;
        }

        private static bool IsReferenceType(SerializedProperty prop) {
            return prop.propertyType is SerializedPropertyType.ManagedReference;
        }
        
        private static bool IsBitField(SerializedProperty prop, out Type enumType) {
            enumType = null;
            if (prop.propertyType != SerializedPropertyType.Enum) {
                return false;
            }

            var fieldInfo = SerializedPropertyHelper.GetFieldInfoFromProperty(prop);
            if (fieldInfo == null) {
                return false;
            }

            enumType = fieldInfo.FieldType;
            return enumType.IsDefined(typeof(FlagsAttribute), true);
        }
    }
}