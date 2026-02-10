using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Broilerplate.Editor.Broilerplate.Data {
    /// <summary>
    /// A dropdown menu that stays open after item selection, useful for multi-select scenarios.
    /// Such as the highly exotic flags enums. Because why else would this not be an existing feature?
    /// </summary>
    public class PersistentDropdownMenu {
        private readonly List<DropdownItem> items = new List<DropdownItem>();

        private PersistentDropdownWindow currentWindow;
        private bool IsClosed {
            get {
                var now = DateTime.UtcNow;
                return (now - timeClosed).TotalMilliseconds > 250 && timeClosed < now;
            }
        }


        private DateTime timeClosed;

        private bool isOpen;

        public void AddItem(string name, bool isChecked, Action action) {
            items.Add(new DropdownItem {
                name = name,
                isChecked = isChecked,
                action = action
            });
        }

        public void ToggleDropDown(Rect position) {
            if ((!isOpen && IsClosed) || (isOpen && IsClosed)) {
                OpenDropDown(position);
            }
            else {
                CloseDropdown();
            }
        }
        
        private void OpenDropDown(Rect position) {
            isOpen = true;
            currentWindow = ScriptableObject.CreateInstance<PersistentDropdownWindow>();
            currentWindow.OnFocusLost += CloseDropdown;
            currentWindow.Initialize(items);
            currentWindow.ShowAsDropDown(GUIUtility.GUIToScreenRect(position), new Vector2(position.width, CalculateHeight()));
        }

        private void CloseDropdown() {
            if (currentWindow) {
                currentWindow.Close();
            }

            timeClosed = DateTime.UtcNow;
            isOpen = false;
        }

        private float CalculateHeight() {
            return Mathf.Min(items.Count * 20 + 10, 400);
        }

        private class DropdownItem {
            public string name;
            public bool isChecked;
            public Action action;
        }

        private class PersistentDropdownWindow : EditorWindow {
            private List<DropdownItem> items;
            private VisualElement rootElement;
            public event Action OnFocusLost;

            public void Initialize(List<DropdownItem> sourceItems) {
                items = new List<DropdownItem>(sourceItems);
                rootElement = rootVisualElement;
                rootElement.style.paddingTop = 4;
                rootElement.style.paddingBottom = 4;

                BuildMenu();
            }

            private void BuildMenu() {
                rootElement.Clear();

                foreach (var item in items) {
                    var itemElement = new VisualElement {
                        style = {
                            flexDirection = FlexDirection.Row,
                            paddingLeft = 10,
                            paddingRight = 10,
                            paddingTop = 3,
                            paddingBottom = 3,
                            alignItems = Align.Center
                        }
                    };

                    // Checkmark
                    var checkmark = new Label(item.isChecked ? "✓" : " ") {
                        style = {
                            width = 16,
                            unityTextAlign = TextAnchor.MiddleCenter
                        }
                    };
                    itemElement.Add(checkmark);

                    // Label
                    var label = new Label(item.name) {
                        style = {
                            flexGrow = 1,
                            unityTextAlign = TextAnchor.MiddleLeft
                        }
                    };
                    itemElement.Add(label);

                    // Hover effect
                    itemElement.RegisterCallback<MouseEnterEvent>(evt => {
                        itemElement.style.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
                    });

                    itemElement.RegisterCallback<MouseLeaveEvent>(evt => {
                        itemElement.style.backgroundColor = Color.clear;
                    });

                    // Click handler
                    itemElement.RegisterCallback<MouseDownEvent>(evt => {
                        if (evt.button == 0) {
                            item.action?.Invoke();
                            item.isChecked = !item.isChecked;

                            // Update the checkmark
                            checkmark.text = item.isChecked ? "✓" : " ";

                            evt.StopPropagation();
                        }
                    });

                    rootElement.Add(itemElement);
                }
            }

            private void OnLostFocus() {
                OnFocusLost?.Invoke();
                // since this is opened as popup, losing focus will destroy the window.
            }
        }
    }
}
