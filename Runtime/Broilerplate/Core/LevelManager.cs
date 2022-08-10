using System;
using System.Collections;
using Broilerplate.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

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
        public static event Action<string> OnLevelUnloaded;
        public static event Action<Scene> BeforeLevelUnload;

        private static string loadingScene;
        
        public static void LoadLevelAsync(string levelName, float loadMultiplier = 1, Action<float> progress = null) {
            CoroutineJobs.StartJob(DoLoadLevelAsync(levelName, loadMultiplier, progress), true);
        }
        
        private static IEnumerator DoLoadLevelAsync(string levelName, float loadMultiplier, Action<float> progress) {
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(loadingScene)) {
                if (loadingScene != currentScene) {
                    yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
                }

                BeforeLevelLoad?.Invoke(levelName);
                AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
                float loadingProgress = 0;
                while (loadingProgress < 1 || !loadNewScene.isDone) {
                    loadingProgress += loadNewScene.progress * loadMultiplier;
                    progress?.Invoke(loadingProgress);
                    yield return null;
                }

                BeforeLevelUnload?.Invoke(SceneManager.GetSceneByName(currentScene));
                yield return SceneManager.UnloadSceneAsync(currentScene);
                OnLevelUnloaded?.Invoke(currentScene);
                if (loadingScene != currentScene) {
                    yield return SceneManager.UnloadSceneAsync(loadingScene);
                }
                
                Resources.UnloadUnusedAssets();
                GC.Collect();
                OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(levelName));
            }
            else {
                var sceneProgress = SceneManager.LoadSceneAsync(levelName); // will unload all previous ofc
                float loadingProgress = 0;
                while (loadingProgress < 1 || !sceneProgress.isDone) {
                    loadingProgress += sceneProgress.progress * loadMultiplier;
                    progress?.Invoke(loadingProgress);
                    yield return null;
                }
                Resources.UnloadUnusedAssets();
                GC.Collect();
                OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(levelName));
            }
            yield return null;
        }



        public static void SetLoadingScene(Scene ls) {
            loadingScene = ls.name;
        }
    }
}