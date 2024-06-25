using System;
using System.Collections.Generic;
using System.IO;
using Broilerplate.Core.Subsystems;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
    /// Very tasty.
    /// This is the configuration from which the whole framework is being constructed.
    /// It will be auto-generated into the Resources folder if not present.
    /// </summary>
    public class BroilerConfiguration : ScriptableObject {
        [Tooltip("When false you will have to call GameInstance.BootstrapWorldForLevel manually. Otherwise it gets called as soon as level is ready.")]
        [SerializeField]
        private bool autoBootstrapWorld = true;
        
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
        [Tooltip("Use this to artificially prolong loading screen time. This is the time in seconds a loading screen will be shown.")]
        private float defaultMinimumLoadingTime = 0;

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

        public float DefaultMinimumLoadingTime => defaultMinimumLoadingTime;

        public bool AutoBootstrapWorld => autoBootstrapWorld;

        /// <summary>
        /// Get a game mode for a given loaded scene.
        /// This is save as it compares the scenes native handles.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public GameMode GetGameModeFor(Scene scene) {
            for (int i = 0; i < gameModeOverrides.Count; ++i) {
                if (gameModeOverrides[i].scene == scene) {
                    return gameModeOverrides[i].gameModeOverridePrefab;
                }
            }
            
            return defaultGameModePrefab;
        }
        
        /// <summary>
        /// Get a game mode for the given scene by scene name.
        /// This may be unreliable as we have to compare substrings and those
        /// are not necessarily unique. Beware of that!
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public GameMode GetGameModeFor(string sceneName) {
            for (int i = 0; i < gameModeOverrides.Count; ++i) {
                if (gameModeOverrides[i].scene.SceneName.Contains(sceneName)) {
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