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
        public static event Action<string> BeforeLevelLoad;
        public static event Action<Scene> OnLevelLoaded;
        public static event Action<Scene> OnLevelLoadedAdditively;
        public static event Action<string> BeforeLevelLoadAdditively;
        public static event Action<string> OnLevelUnloaded;
        public static event Action<Scene> BeforeLevelUnload;

        private static Scene loadingScene;

        /// <summary>
        /// Loads in loading scene,
        /// unloads current level, then loads new level.
        /// </summary>
        /// <param name="levelName"></param>
        public static void LoadLevel(string levelName) {
            // if we need to load levels from asset bundles, this is the place to check for them
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(loadingScene.name)) {
                var loadTask = SceneManager.LoadSceneAsync(loadingScene.name, LoadSceneMode.Additive);
                loadTask.completed += _ => {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    HandleLoadLevel(levelName, currentScene);
                };
            }
            else
            {
                HandleLoadLevel(levelName, currentScene, false);
            }
        }

        public static void SetLoadingScene(Scene ls)
        {
            loadingScene = ls;
        }

        /// <summary>
        /// Loads a level additively without unloading anything or entering a loading scene.
        /// </summary>
        /// <param name="levelName"></param>
        public static void LoadLevelAdditive(string levelName) {
            // if we need to load levels from asset bundles, this is the place to check for them
            BeforeLevelLoadAdditively?.Invoke(levelName);
            var task = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            task.completed += _ => {
                OnLevelLoadedAdditively?.Invoke(SceneManager.GetSceneByName(levelName));
            };
        }

        private static void HandleLoadLevel(string targetScene, string unloadScene, bool unloadLoadingScene = true) {
            BeforeLevelLoad?.Invoke(targetScene);
            var task = SceneManager.LoadSceneAsync(targetScene,
                unloadLoadingScene ? LoadSceneMode.Additive : LoadSceneMode.Single);
            task.completed += _ => {
                if (unloadLoadingScene) {
                    BeforeLevelUnload?.Invoke(SceneManager.GetSceneByName(unloadScene));
                    var unloadOldSceneTask = SceneManager.UnloadSceneAsync(unloadScene);
                    unloadOldSceneTask.completed += _ => {
                        OnLevelUnloaded?.Invoke(unloadScene);
                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                        // we call this after the loading scene is gone to please the unitar.
                        // It would otherwise get confused if, for instance, new gameobjects were created
                        // before the loading scene is gone.
                        // Now: We could set the active scene. But for reasons unknown, that doesn't always work.
                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));
                        OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(targetScene));
                    };
                }
                else {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    OnLevelUnloaded?.Invoke(unloadScene);
                    OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(targetScene));
                    
                }
            };

        }
    }
}