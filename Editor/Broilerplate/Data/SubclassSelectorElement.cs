using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Broilerplate.Editor.Broilerplate.Data {
    /// <summary>
    /// A visual element that provides a dropdown to select and instantiate subclasses
    /// for SerializeReference fields (managed references).
    /// </summary>
    public class SubclassSelectorElement : VisualElement {
        private readonly SerializedProperty property;
        private Button typeSelector;
        private VisualElement container;
        private readonly List<Type> availableTypes;

        public SubclassSelectorElement(SerializedProperty property) {
            this.property = property;

            style.marginBottom = 5;

            // Get the base type from the managed reference
            var baseType = GetManagedReferenceType();

            if (baseType == null) {
                var errorLabel = new Label("Unable to determine base type for managed reference") {
                    style = {
                        color = Color.red
                    }
                };
                Add(errorLabel);
                return;
            }
            availableTypes = FindAssignableTypes(baseType);
            BuildTypeSelector();
        }

        private void BuildTypeSelector() {
            container = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 5
                }
            };

            var label = new Label(property.displayName) {
                style = {
                    minWidth = 120,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            container.Add(label);

            // Determine current type display name
            string currentTypeName = GetCurrentTypeDisplayName();

            typeSelector = new Button(ShowTypeSelectionWindow) {
                text = currentTypeName,
                style = {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            container.Add(typeSelector);

            Add(container);
        }

        private string GetCurrentTypeDisplayName() {
            if (property.managedReferenceValue == null) {
                return "<null>";
            }

            Type currentType = property.managedReferenceValue.GetType();
            return GetShortTypeName(currentType);
        }

        private void ShowTypeSelectionWindow() {
            var dropdown = new TypeSelectionDropdown(
                new AdvancedDropdownState(),
                availableTypes,
                OnTypeSelected
            );

            var buttonRect = typeSelector.worldBound;
            dropdown.Show(buttonRect);
        }

        private void OnTypeSelected(Type selectedType) {
            property.serializedObject.Update();

            if (selectedType == null) {
                property.managedReferenceValue = null;
            }
            else {
                // Instantiate the selected type
                object instance = Activator.CreateInstance(selectedType);
                property.managedReferenceValue = instance;
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);

            typeSelector.text = GetCurrentTypeDisplayName();
        }
        

        private Type GetManagedReferenceType() {
            // Get the type from the managedReferenceFieldTypename
            string typeName = property.managedReferenceFieldTypename;

            if (string.IsNullOrEmpty(typeName)) {
                return null;
            }

            // Format is "AssemblyName TypeFullName"
            string[] parts = typeName.Split(' ');
            if (parts.Length != 2) {
                return null;
            }

            string assemblyName = parts[0];
            string typeFullName = parts[1];

            // Try to load the assembly and get the type
            Assembly assembly = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                if (asm.GetName().Name == assemblyName) {
                    assembly = asm;
                    break;
                }
            }

            if (assembly == null) {
                return null;
            }

            return assembly.GetType(typeFullName);
        }

        private List<Type> FindAssignableTypes(Type baseType) {
            var types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                // There are no types in these, that I would find interesting and they are MASSIVE libraries
                string assemblyName = assembly.GetName().Name;
                if (assemblyName.StartsWith("Unity") ||
                    assemblyName.StartsWith("System") ||
                    assemblyName.StartsWith("mscorlib") ||
                    assemblyName.StartsWith("netstandard")) {
                    continue;
                }

                try {
                    foreach (var type in assembly.GetTypes()) {
                        // Check if type is assignable to base type and weed out anything that can't be instantiated
                        if (!baseType.IsAssignableFrom(type)) {
                            continue;
                        }

                        // Must be a class and have Serializable attribute or be marked as serializable
                        if (!type.IsClass) {
                            continue;
                        }

                        // Skip abstract classes
                        if (type.IsAbstract) {
                            continue;
                        }

                        // Must have a parameterless constructor
                        if (type.GetConstructor(Type.EmptyTypes) == null) {
                            continue;
                        }

                        // Check if it has [Serializable] attribute
                        if (type.GetCustomAttribute<SerializableAttribute>() != null ||
                            !type.IsSerializable) {
                            // For Unity serialization, we actually don't need [Serializable]
                            // Unity uses its own serialization for managed references
                            types.Add(type);
                        }
                        else {
                            types.Add(type);
                        }
                    }
                }
                catch {
                    // Skip assemblies that throw exceptions when getting types. Thems are weird
                }
            }

            // Sort by name for easier browsing
            types.Sort((a, b) => string.Compare(GetFullTypeName(a), GetFullTypeName(b), StringComparison.Ordinal));

            return types;
        }

        private string GetShortTypeName(Type type) {
            return type.Name;
        }

        private string GetFullTypeName(Type type) {
            if (type.Namespace != null && !string.IsNullOrEmpty(type.Namespace)) {
                return $"{type.Namespace}.{type.Name}";
            }
            return type.Name;
        }
    }

    /// <summary>
    /// A searchable dropdown window for selecting types.
    /// Uses Unity's AdvancedDropdown for built-in search functionality.
    /// </summary>
    internal class TypeSelectionDropdown : AdvancedDropdown {
        private readonly List<Type> types;
        private readonly Action<Type> onTypeSelected;
        private readonly Dictionary<int, Type> idToType = new Dictionary<int, Type>();
        private int currentId = 1;

        public TypeSelectionDropdown(AdvancedDropdownState state, List<Type> types, Action<Type> onTypeSelected)
            : base(state) {
            this.types = types;
            this.onTypeSelected = onTypeSelected;
            minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() {
            var root = new AdvancedDropdownItem("Select Type");

            // Add null option
            var nullItem = new AdvancedDropdownItem("<null>") {
                id = 0
            };
            root.AddChild(nullItem);
            idToType[0] = null;

            // Group types by namespace
            var namespaceGroups = types
                .GroupBy(t => string.IsNullOrEmpty(t.Namespace) ? "<Global>" : t.Namespace)
                .OrderBy(g => g.Key);

            foreach (var group in namespaceGroups) {
                if (group.Count() == 1 && group.Key != "<Global>") {
                    // If namespace has only one type, add it directly to root
                    var type = group.First();
                    var item = new AdvancedDropdownItem(type.Name) {
                        id = currentId
                    };
                    idToType[currentId] = type;
                    currentId++;
                    root.AddChild(item);
                }
                else {
                    // Create a namespace folder
                    var namespaceItem = new AdvancedDropdownItem(group.Key);

                    foreach (var type in group.OrderBy(t => t.Name)) {
                        var typeItem = new AdvancedDropdownItem(type.Name) {
                            id = currentId
                        };
                        idToType[currentId] = type;
                        currentId++;
                        namespaceItem.AddChild(typeItem);
                    }

                    root.AddChild(namespaceItem);
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            if (idToType.TryGetValue(item.id, out Type selectedType)) {
                onTypeSelected?.Invoke(selectedType);
            }
        }
    }
}