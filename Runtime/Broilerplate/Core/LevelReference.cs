using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core
{
    // https://github.com/JohannesMP/unity-scene-reference
    [Serializable]
    public class LevelReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset sceneAsset;
        private bool IsValidScene => sceneAsset != null;
#endif

        [SerializeField] private string scenePath;

        public string ScenePath
        {
            get
            {
#if UNITY_EDITOR
                return GetScenePathFromAsset();
#else
                return scenePath;
#endif
            }
            set
            {
                scenePath = value;
#if UNITY_EDITOR
                sceneAsset = GetSceneAssetFromPath();
#endif
            }
        }

        public string SceneName => this; // invokes implicit to-string operator

        public static implicit operator string(LevelReference levelReference)
        {
            string name = Path.GetFileName(levelReference.ScenePath);
            if (string.IsNullOrEmpty(name)) {
                return default;
            }
            int unity = name.LastIndexOf(".unity", StringComparison.Ordinal);
            name = name.Substring(0, unity);
            return name;
        }

        public static implicit operator Scene(LevelReference levelReference)
        {
            string name = Path.GetFileName(levelReference.ScenePath);
            
            if (string.IsNullOrEmpty(name)) {
                return default;
            }
            
            int unity = name.LastIndexOf(".unity", StringComparison.Ordinal);
            name = name.Substring(0, unity);
            
            return SceneManager.GetSceneByName(name);
        }

        public LevelReference()
        {
            // unitar ctor
        }

        public LevelReference(Scene scene)
        {
            ScenePath = scene.path;
        }

        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }

#if UNITY_EDITOR
        private SceneAsset GetSceneAssetFromPath()
        {
            return string.IsNullOrEmpty(scenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }

        private string GetScenePathFromAsset()
        {
            return sceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(sceneAsset);
        }

        private void HandleBeforeSerialize()
        {
            // Asset is invalid but have Path to try and recover from
            if (!IsValidScene && string.IsNullOrEmpty(scenePath) == false)
            {
                sceneAsset = GetSceneAssetFromPath();
                if (sceneAsset == null)
                {
                    scenePath = string.Empty;
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
            // Asset takes precendence and overwrites Path
            else
            {
                scenePath = GetScenePathFromAsset();
            }
        }

        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (IsValidScene) return;

            // Asset is invalid but have path to try and recover from
            if (string.IsNullOrEmpty(scenePath)) return;

            sceneAsset = GetSceneAssetFromPath();
            // No asset found, path was invalid. Make sure we don't carry over the old invalid path
            if (!sceneAsset) scenePath = string.Empty;

            if (!Application.isPlaying) UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}