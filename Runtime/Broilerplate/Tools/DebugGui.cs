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
                return contents.GetHashCode() * 31 + color.GetHashCode();
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
        private List<DisplayInfo> displayList = new List<DisplayInfo>();

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
                var actualText = new GUIContent($"<color=#{ColorUtility.ToHtmlStringRGB(key.color)}>{key.contents}</color>");
                var shadowText = new GUIContent($"<color=#444>{key.contents}</color>");
                float y = num++;
                GUI.Label(new Rect(10.5f, (20 * y) + .5f, 350f, 50f), shadowText, s);
                GUI.Label(new Rect(10f, (20 * y), 350f, 50f), actualText, s);
                if (Time.time > key.timeToRemove) {
                    displayList.Remove(key);
                    i--;
                }
            }
        }
#endif
    }
}