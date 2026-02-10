using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Broilerplate.Editor.Broilerplate.Data {
    public class KombinatEnumField : VisualElement {
        private readonly Button button;
        private readonly VisualElement popup;
        private readonly SerializedProperty property;
        private readonly Type enumType;

        private Enum memoryValue;

        public KombinatEnumField(SerializedProperty prop, Enum initialValue) {
            property = prop;
            enumType = initialValue.GetType();
            memoryValue = initialValue;

            button = new Button(TogglePopup);
            button.style.flexGrow = 1;
            Add(button);

            popup = new VisualElement();
            popup.style.position = Position.Absolute;
            popup.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            popup.style.borderBottomLeftRadius = 4;
            popup.style.borderBottomRightRadius = 4;
            popup.style.display = DisplayStyle.None;
            popup.BringToFront();
            // popup.style.zIndex = 1000;

            Add(popup);

            BuildPopup();
            UpdateButtonText();
        }

        void TogglePopup() {
            popup.style.display =
                popup.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        void BuildPopup() {
            popup.Clear();

            var values = Enum.GetValues(enumType);

            foreach (Enum val in values) {
                var toggle = new Toggle(val.ToString());
                toggle.value = HasFlag(val);

                toggle.RegisterValueChangedCallback(evt => {
                    SetFlag(val, evt.newValue);
                    UpdateButtonText();
                    property.serializedObject.ApplyModifiedProperties();
                });

                popup.Add(toggle);
            }
        }

        bool HasFlag(Enum flag) {
            return memoryValue.HasFlag(flag);
        }

        void SetFlag(Enum flag, bool enabled) {
            ulong c = Convert.ToUInt64(memoryValue);
            ulong f = Convert.ToUInt64(flag);

            c = enabled ? (c | f) : (c & ~f);
            memoryValue = (Enum)Enum.ToObject(enumType, c);
            property.boxedValue = memoryValue;
        }

        void UpdateButtonText() {
            button.text = Enum.Format(enumType, memoryValue, "F");
        }
    }
}