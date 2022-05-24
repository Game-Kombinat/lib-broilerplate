using System;
using System.Collections.Generic;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace Broilerplate.Tools {
    
    public class DebugGui : MonoBehaviour {
        private readonly struct DisplayInfo {
            public readonly float timeToRemove;
            public readonly string contents;
            public readonly Color color;

            public DisplayInfo(string txt, float removalTime, Color c) {
                timeToRemove = removalTime;
                contents = txt;
                color = c;
            }

            public bool Equals(DisplayInfo other) {
                return timeToRemove.Equals(other.timeToRemove) && contents == other.contents && color.Equals(other.color);
            }

            public override bool Equals(object obj) {
                return obj is DisplayInfo other && Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(timeToRemove, contents, color);
            }
        }
        private static DebugGui instance;
        private static DebugGui Instance {
            get {
                if (instance == null) {
                    var go = new GameObject("Debug Gui");
                    go.transform.position = Vector3.zero;
                    instance = go.AddComponent<DebugGui>();
                }

                return instance;
            }
        }
        private List<DisplayInfo> displayList = new();

        public static void Print(string contents, float duration = 0) {
            Print(contents, Color.gray, duration);
        }
        
        public static void Print(string contents, Color displayColor) {
            Print(contents, displayColor, 0);
        }
        
        public static void Print(string contents, Color displayColor, float duration) {
            var di = new DisplayInfo(contents, Time.time + duration , displayColor);
            Instance.displayList.Add(di);
            // if (!Instance.displayList.Contains(di)) {
            //     Instance.displayList.Add(di);
            // }
        }
        
#if UNITY_EDITOR
        private void OnGUI() {
            if (displayList.Count == 0) {
                return;
            }

            int num = 1;
            for (var i = 0; i < displayList.Count; i++) {
                var key = displayList[i];
                GUIStyle s = new GUIStyle {
                    richText = true
                };
                var gc = new GUIContent($"<color=#{ColorUtility.ToHtmlStringRGB(key.color)}>{key.contents}</color>");
                GUI.Label(new Rect(10f, (20 * num++), 350f, 50f), gc, s);
                if (Time.time > key.timeToRemove) {
                    displayList.Remove(key);
                    i--;
                }
            }
        }
#endif
    }
}