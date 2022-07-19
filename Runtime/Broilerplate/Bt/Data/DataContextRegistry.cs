using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameKombinat.ControlFlow.Bt.Data {

    [Serializable]
    public enum DataContextScope {
        Game,
        Scene,
        Graph
    }
    
    [Serializable]
    public class SceneToDataMap {
        public string sceneName;
        public DataContext data;
    }
    /// <summary>
    /// A registry that contains a reference to the project global data context
    /// and additionally, per scene, a mapping to the scenes data contexts.
    /// </summary>
    [Serializable]
    public class DataContextRegistry : ScriptableObject {
        [SerializeField]
        private DataContext globalDataContext;

        [SerializeField]
        private List<SceneToDataMap> sceneData = new List<SceneToDataMap>();

        public DataContext GetGlobalContext() {
            #if UNITY_EDITOR
            if (globalDataContext == null) {
                globalDataContext = CreateInstance<DataContext>();
                globalDataContext.name = "Global Game Data Context";
                AssetDatabase.AddObjectToAsset(globalDataContext, this);
                MarkDirty();
                AssetDatabase.SaveAssets();
            }
            #endif
            return globalDataContext;
        }

        public DataContext GetSceneDataContext(string sceneName) {
            for (int i = 0; i < sceneData.Count; ++i) {
                var sdata = sceneData[i];
                if (sdata.sceneName == sceneName) {
                    return sdata.data;
                }
            }

            #if UNITY_EDITOR
            // In Editor, we will create new scene data on the fly if there is none yet.
            var newDc = CreateInstance<DataContext>();
            newDc.name = $"{sceneName} Data Context";
            var newData = new SceneToDataMap {
                data = newDc,
                sceneName = sceneName,
            };
            sceneData.Add(newData);
            AssetDatabase.AddObjectToAsset(newData.data, this);

            MarkDirty();
            AssetDatabase.SaveAssets();
            return newData.data;
            #else
            Debug.LogError($"No Data Context for scene {sceneName}");
            return null;
            #endif
            
        }
        
        #if UNITY_EDITOR
        public void MarkDirty() {
            EditorUtility.SetDirty(this);
        }
        #endif

        /*
         * Note on this:
         * We Could use zenject or something like it to inject the Registry where we need it.
         * Unfortunately though, we need it at editor time as well.
         * As such, we'll have to make do with a singleton.
         */
        public static DataContextRegistry Instance {
            get {
                #if UNITY_EDITOR
                // At editor time, for convenience, we make sure that this will be generated.
                // Since this is a globally accessible asset, this will happen only once.
                // Come the time a game is deployed, the possibility that this does not exist is precisely 0
                var i = Resources.Load<DataContextRegistry>("Data/DataContextRegistry");
                if (i == null) {
                    if (!Directory.Exists("Assets/Resources/Data")) {
                        Directory.CreateDirectory("Assets/Resources/Data");
                    }
                    Debug.Log("Creating new registry because I think it doesn't exist");
                    AssetDatabase.CreateAsset(CreateInstance<DataContextRegistry>(), @"Assets/Resources/Data/DataContextRegistry.asset");
                    AssetDatabase.SaveAssets();
                }
                #endif
                // Load is actually returning the same instance every time.
                return Resources.Load<DataContextRegistry>("Data/DataContextRegistry");
            }
        }

    }
}