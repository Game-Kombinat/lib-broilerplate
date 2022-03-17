using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    /// <summary>
    /// Wraps around the Unity SceneManager and exposes a couple
    /// things that give you more control on scene handling.
    ///
    /// </summary>
    public class LevelManager {
        /// Bunch of actions that expose hooks in the scene loading process.
        /// These are not called for the loading scene.
        /// Loading scene is not invoked for additive level load requests.
        public static event Action<Scene> BeforeLevelLoad;
        public static event Action<Scene> OnLevelLoaded;
        public static event Action<Scene> OnLevelLoadedAdditively;
        public static event Action<Scene> BeforeLevelLoadAdditively;
        public static event Action<Scene> OnLevelUnloaded;
        public static event Action<Scene> BeforeLevelUnload;

        private static Scene loadingScene;

        /// <summary>
        /// Loads in loading scene,
        /// unloads current level, then loads new level.
        /// </summary>
        /// <param name="levelName"></param>
        public static void LoadLevel(string levelName) {
            // if we need to load levels from asset bundles, this is the place to check for them
            Scene targetScene = SceneManager.GetSceneByName(levelName);
            Scene currentScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(loadingScene.name)) {
                var loadTask = SceneManager.LoadSceneAsync(loadingScene.name, LoadSceneMode.Additive);
                loadTask.completed += _ => {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    HandleLoadLevel(targetScene, currentScene);
                };
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
            HandleLoadLevel(targetScene, currentScene, false);
        }

        /// <summary>
        /// Loads a level additively without unloading anything or entering a loading scene.
        /// </summary>
        /// <param name="levelName"></param>
        public static void LoadLevelAdditive(string levelName) {
            // if we need to load levels from asset bundles, this is the place to check for them
            Scene targetScene = SceneManager.GetSceneByName(levelName);
            BeforeLevelLoadAdditively?.Invoke(targetScene);
            var task = SceneManager.LoadSceneAsync(targetScene.name, LoadSceneMode.Additive);
            task.completed += _ => {
                OnLevelLoadedAdditively?.Invoke(targetScene);
            };
        }

        private static void HandleLoadLevel(Scene targetScene, Scene unloadScene, bool unloadLoadingScene = true) {
            var activeScene = SceneManager.GetActiveScene();
            BeforeLevelUnload?.Invoke(activeScene);
            // unload current scene and register a complete callback
            var task = SceneManager.UnloadSceneAsync(unloadScene);
            task.completed += a => {
                // invoke some events accordingly
                OnLevelUnloaded?.Invoke(activeScene);
                BeforeLevelLoad?.Invoke(targetScene);
                
                // load target scene and register complete callback
                var loadTask = SceneManager.LoadSceneAsync(targetScene.name, unloadLoadingScene ? LoadSceneMode.Additive : LoadSceneMode.Single);
                loadTask.completed += b => {
                    // call final level loaded event.
                    OnLevelLoaded?.Invoke(targetScene);
                    
                    // unload loading scene if it was requested.
                    if (unloadLoadingScene) {
                        SceneManager.UnloadSceneAsync(loadingScene);
                    }
                };
            };
        }
    }
}