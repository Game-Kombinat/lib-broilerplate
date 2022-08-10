using System;
using System.Collections.Generic;
using System.IO;
using Broilerplate.Core.Subsystems;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    [Serializable]
    class MapGameModeOverrides {
        [SerializeField]
        public LevelReference scene;

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

        [Header("Scene Loading")]
        [SerializeField]
        private LevelReference startupScene;
        
        [SerializeField]
        private LevelReference loadingScene;
        
        [SerializeField]
        [Tooltip("Use this to increase the loading times (if you artificially want to show loading screen for longer for some reason)")]
        [Range(0, 1)]
        private float defaultSceneLoadMultiplier = 1;

        [SerializeField]
        private List<MapGameModeOverrides> gameModeOverrides = new List<MapGameModeOverrides>();
        
        [Header("Subsystems")]
        [SerializeField]
        private List<WorldSubsystem> worldSubsystems = new List<WorldSubsystem>();

        public GameInstance GameInstanceType => defaultGameInstancePrefab == null ? CreateInstance<GameInstance>() : defaultGameInstancePrefab;

        public World WorldType => defaultWorldPrefab == null ? CreateInstance<World>() : defaultWorldPrefab;

        public List<WorldSubsystem> WorldSubsystems => worldSubsystems;

        public LevelReference LoadingScene => loadingScene;

        public LevelReference StartupScene => startupScene;

        public float DefaultSceneLoadMultiplier => defaultSceneLoadMultiplier;

        public GameMode GetGameModeFor(Scene scene) {
            for (int i = 0; i < gameModeOverrides.Count; ++i) {
                if (gameModeOverrides[i].scene == scene) {
                    return gameModeOverrides[i].gameModeOverridePrefab;
                }
            }
            
            return defaultGameModePrefab;
        }

        public void RegisterSceneOverride(Scene scene, GameMode gameModeOverride) {
            for (int i = 0; i < gameModeOverrides.Count; i++) {
                if (gameModeOverrides[i].scene == scene) {
                    gameModeOverrides[i].gameModeOverridePrefab = gameModeOverride;
                    return;
                }
            }
            // new
            gameModeOverrides.Add(new MapGameModeOverrides() {scene = new LevelReference(scene), gameModeOverridePrefab = gameModeOverride});
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