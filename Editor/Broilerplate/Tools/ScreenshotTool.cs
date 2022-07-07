using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Broilerplate.Tools {
    public class ScreenshotTool : EditorWindow {
        [MenuItem("Game Kombinat/Screenshot Tool")]
        public static void OpenEd() {
            var win = EditorWindow.CreateWindow<ScreenshotTool>();
            win.Load();
            var pos = win.position;
            pos.width = 300;
            pos.height = 100;
            win.position = pos;
            win.titleContent = new GUIContent("Screenshot Tool");
            win.Show();
        }

        private ScreenshotToolData configuration;

        private void Load() {
            string fileName = "UserSettings/ScreenshotToolConfig.json";
            configuration = CreateInstance<ScreenshotToolData>();
            if (File.Exists("UserSettings/ScreenshotToolConfig.json")) {
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(fileName), configuration);
            }
            else
            {
                File.WriteAllText(fileName, EditorJsonUtility.ToJson(configuration));
            }
        }

        private void Save() {
            string fileName = "UserSettings/ScreenshotToolConfig.json";
            File.WriteAllText(fileName, EditorJsonUtility.ToJson(configuration));
        }


        private void OnGUI() {
            EditorGUILayout.BeginVertical(GUILayout.Height(100), GUILayout.Width(250));

            DrawScreenshotPathCombo();
            DrawMultiplier();

            if (GUILayout.Button("Take Screenshot"))
            {
                var dt = DateTime.Now;
                string file = configuration.screenshotPath;
                file += "/Screenshot_" + dt.ToString("yy-MM-dd-hh-mm-ss") + ".png"; 
                ScreenCapture.CaptureScreenshot(file, configuration.resolutionMultiplier);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawMultiplier()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resolution Multiplier:", GUILayout.Width(150));
            configuration.resolutionMultiplier = EditorGUILayout.IntField(configuration.resolutionMultiplier, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            if (configuration.resolutionMultiplier > 4)
            {
                EditorGUILayout.HelpBox("Large Multipliers may slow down or downright crash your graphics device. Beware!", MessageType.Warning);
            }
        }

        private void DrawScreenshotPathCombo()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path:", GUILayout.Width(30));
            EditorGUILayout.LabelField(configuration.screenshotPath, GUILayout.Width(120));
            if (EditorGUILayout.LinkButton("Change"))
            {
                configuration.screenshotPath = EditorUtility.OpenFolderPanel("Select Screenshot Directory", configuration.screenshotPath, "");
                configuration.screenshotPath = configuration.screenshotPath.Substring(configuration.screenshotPath.IndexOf("Assets", StringComparison.Ordinal));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            Save();
        }
    }
}