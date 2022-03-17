using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    [Serializable]
    class MapGameModeOverrides {
        [SerializeField]
        public Scene scene;

        [SerializeField]
        public GameMode gameModeOverridePrefab;
    }
    
    /// <summary>
    /// Configures the Broiler!
    /// </summary>
    public class BroilerConfiguration : ScriptableObject {
        [SerializeField]
        private GameMode defaultGameModePrefab;

        [SerializeField]
        private GameInstance defaultGameInstancePrefab;
        
        [SerializeField]
        private World defaultWorldPrefab;

        [SerializeField]
        private List<MapGameModeOverrides> gameModeOverrides = new();

        public GameInstance GameInstanceType => defaultGameInstancePrefab == null ? CreateInstance<GameInstance>() : defaultGameInstancePrefab;

        public World WorldType => defaultWorldPrefab == null ? CreateInstance<World>() : defaultWorldPrefab;

        public GameMode GetGameModeFor(Scene scene) {
            for (int i = 0; i < gameModeOverrides.Count; ++i) {
                if (gameModeOverrides[i].scene == scene) {
                    return gameModeOverrides[i].gameModeOverridePrefab;
                }
            }
            
            return defaultGameModePrefab;
        }

        public static BroilerConfiguration GetConfiguration() {
            var config = Resources.Load<BroilerConfiguration>("BroilerGameConfiguration");
            #if UNITY_EDITOR
            if (!config) {
                config = CreateInstance<BroilerConfiguration>();
                if (!Directory.Exists("Assets/Resources")) {
                    Directory.CreateDirectory("Assets/Resources");
                }
                AssetDatabase.CreateAsset(config, "Assets/Resources/BroilerGameConfiguration.asset");
                AssetDatabase.SaveAssets();
            }
            #endif
            return config;
        }
    }
}