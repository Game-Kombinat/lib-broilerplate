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

        private static string loadingScene;

        /// <summary>
        /// Loads in loading scene,
        /// unloads current level, then loads new level.
        /// </summary>
        /// <param name="levelName"></param>
        public static void LoadLevel(string levelName) {
            // if we need to load levels from asset bundles, this is the place to check for them
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(loadingScene)) {
                if (loadingScene != currentScene) {
                    var loadTask = SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
                    loadTask.completed += _ => {
                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                        HandleLoadLevel(levelName, currentScene);
                    };  
                }
                else {
                    // If we booted into the loading scene, or don't have any, just load new one
                    HandleLoadLevel(levelName, currentScene, false);
                }

            }
            else {
                // If we booted into the loading scene, or don't have any, just load new one
                HandleLoadLevel(levelName, currentScene, false);
            }
        }

        public static void SetLoadingScene(Scene ls) {
            loadingScene = ls.name;
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
            var loadNewScene = SceneManager.LoadSceneAsync(targetScene,
                unloadLoadingScene ? LoadSceneMode.Additive : LoadSceneMode.Single);
            loadNewScene.completed += _ => {
                BeforeLevelUnload?.Invoke(SceneManager.GetSceneByName(unloadScene));
                // very important, it won't let us unload the loading scene otherwise if we're starting fresh
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));
                if (unloadLoadingScene) { // if this was false we had loaded the new scene as single, that means unity auto unloaded all other scenes.
                    var unloadOldSceneTask = SceneManager.UnloadSceneAsync(unloadScene);
                    unloadOldSceneTask.completed += _ => {
                        OnLevelUnloaded?.Invoke(unloadScene);
                        // don't much care to wait for this, it oughta be quick.
                        var lsUnload = SceneManager.UnloadSceneAsync(loadingScene);
                        lsUnload.completed += _ => {
                            Resources.UnloadUnusedAssets();
                            GC.Collect();
                            // we call this after the loading scene is gone to please the unitar.
                            // It would otherwise get confused if, for instance, new gameobjects were created
                            // before the loading scene is gone.
                            OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(targetScene));
                        };
                    
                        
                    };
                }
                else {
                    OnLevelUnloaded?.Invoke(unloadScene);
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(targetScene));
                }
            };

        }
    }
}