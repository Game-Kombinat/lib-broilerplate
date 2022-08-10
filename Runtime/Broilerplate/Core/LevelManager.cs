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
        
        public static void LoadLevelAsync(string levelName, Action<float> progress = null, float minimumLoadingTime = -1) {
            if (minimumLoadingTime < 0) {
                minimumLoadingTime = GameInstance.GetInstance().GameInstanceConfiguration.DefaultMinimumLoadingTime;
            }
            CoroutineJobs.StartJob(DoLoadLevelAsync(levelName, minimumLoadingTime, progress), true);
        }
        
        private static IEnumerator DoLoadLevelAsync(string levelName, float fakeLoadingTime, Action<float> progress) {
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(loadingScene)) {
                if (loadingScene != currentScene) {
                    yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
                }

                BeforeLevelLoad?.Invoke(levelName);
                AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
                yield return LoadingLoop(fakeLoadingTime, progress, loadNewScene);

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
                yield return LoadingLoop(fakeLoadingTime, progress, sceneProgress);
                Resources.UnloadUnusedAssets();
                GC.Collect();
                OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(levelName));
            }
            yield return null;
        }

        private static IEnumerator LoadingLoop(float fakeLoadingTime, Action<float> progress, AsyncOperation loadNewScene) {
            float loadingProgress = 0;
            float fakeProgress = 0;
            do {
                if (fakeLoadingTime > 0) {
                    fakeProgress += Time.deltaTime;
                    loadingProgress = Mathf.InverseLerp(0, fakeLoadingTime, fakeProgress);
                }
                else {
                    loadingProgress = loadNewScene.progress;
                }
                progress?.Invoke(loadingProgress);

                yield return null;
            } while (loadingProgress < 1 || !loadNewScene.isDone);
        }


        public static void SetLoadingScene(Scene ls) {
            loadingScene = ls.name;
        }
    }
}