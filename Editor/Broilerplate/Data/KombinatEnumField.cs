using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Broilerplate.Editor.Broilerplate.Data {
    public class KombinatEnumField : VisualElement {
        private readonly Button button;
        private readonly SerializedProperty property;
        private readonly Type enumType;

        private Enum memoryValue;
        private PersistentDropdownMenu menu;

        public KombinatEnumField(SerializedProperty prop, Enum initialValue, string label) {
            property = prop;
            enumType = initialValue.GetType();
            memoryValue = initialValue;

            var root = new VisualElement() {
                style = {
                    flexDirection = FlexDirection.Row
                }
            };
            Add(root);
            
            button = new Button() {
                style = {
                    flexGrow = 1,
                    flexShrink = 0,
                    flexBasis = 0,
                    flexDirection = FlexDirection.Row,
                    paddingLeft = 10,
                    paddingRight = 10,
                }
            };
            button.clicked += ToggleDropdownMenu;

            root.Add(new Label(label) {
                style = {
                    flexGrow = 1,
                    flexShrink = 0,
                    flexBasis = 0,
                    flexDirection = FlexDirection.Row
                }
            });
            root.Add(button);
            BuildDropdownMenuContents();
            UpdateButtonText();
        }
        
        private void ToggleDropdownMenu() {
            menu.ToggleDropDown(button.worldBound);
        }

        private void BuildDropdownMenuContents() {
            menu = new PersistentDropdownMenu();
            var values = Enum.GetValues(enumType);

            foreach (Enum val in values) {
                var flag = val;
                var isChecked = HasFlag(flag);

                menu.AddItem(flag.ToString(), isChecked, () => {
                    SetFlag(flag, !HasFlag(flag));
                    UpdateButtonText();
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
        }

        private bool HasFlag(Enum flag) {
            return memoryValue.HasFlag(flag);
        }

        private void SetFlag(Enum flag, bool enabled) {
            ulong c = Convert.ToUInt64(memoryValue);
            ulong f = Convert.ToUInt64(flag);

            c = enabled ? (c | f) : (c & ~f);
            memoryValue = (Enum)Enum.ToObject(enumType, c);
            property.boxedValue = memoryValue;
        }

        private void UpdateButtonText() {
            button.text = Enum.Format(enumType, memoryValue, "F");
        }
    }
}