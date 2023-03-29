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
        
        // this has a scene object because loaded level may require more meta information
        // and of course because we can.
        public static event Action<Scene> OnLevelLoaded;
        public static event Action<string> OnLevelUnloaded;
        public static event Action<string> BeforeLevelUnload;

        private static string loadingScene;
        public static Scene ActiveScene => SceneManager.GetActiveScene();

        private static bool loadingInProgress;

        private static string currentTargetLevel;

        public static void LoadLevelAsync(string levelName, Action<float> progress = null, float minimumLoadingTime = -1) {
            if (loadingInProgress) {
                Debug.LogWarning($"Attempting to load level {levelName} while {currentTargetLevel} is currently being loaded. Aborting this.");
                return;
            }
            if (minimumLoadingTime < 0) {
                minimumLoadingTime = GameInstance.GetInstance().GameInstanceConfiguration.DefaultMinimumLoadingTime;
            }
            CoroutineJobs.StartJob(DoLoadLevelAsync(levelName, minimumLoadingTime, progress), true);
        }
        
        private static IEnumerator DoLoadLevelAsync(string targetLevelName, float fakeLoadingTime, Action<float> progress) {
            loadingInProgress = true;
            var currentSceneObject = SceneManager.GetActiveScene();
            string currentSceneName = currentSceneObject.name;
            currentTargetLevel = targetLevelName;
            
            if (!string.IsNullOrEmpty(loadingScene)) {
                bool currentIsLoadingScene = true;
                // if we have a loading scene configured, load this in first
                if (loadingScene != currentSceneName) {
                    currentIsLoadingScene = false;
                    yield return LoadLoadingScene();
                }

                if (!currentIsLoadingScene) {
                    // unload currently loaded level if we didn't start from the loading scene
                    yield return UnloadLevelRoutine(currentSceneName);
                }
                // Now load the new scene and get rid of the loading scene if necessary
                yield return LoadLevelRoutine(targetLevelName, fakeLoadingTime, progress, !currentIsLoadingScene);
            }
            else {
                yield return LoadLevelRoutine(targetLevelName, fakeLoadingTime, progress, false);
            }
            loadingInProgress = false;
            currentTargetLevel = null;
        }

        private static IEnumerator LoadLevelRoutine(string targetLevelName, float fakeLoadingTime, Action<float> progress, bool unloadLoadingScene) {
            // if we have a loading scene, we need to load target on top. If we have none, just load as is
            var loadMode = unloadLoadingScene ? LoadSceneMode.Additive : LoadSceneMode.Single;
            BeforeLevelLoad?.Invoke(targetLevelName);
            yield return LoadingLoop(fakeLoadingTime, progress, SceneManager.LoadSceneAsync(targetLevelName, loadMode));
            Resources.UnloadUnusedAssets();
            GC.Collect();
            if (unloadLoadingScene) {
                yield return SceneManager.UnloadSceneAsync(loadingScene);
            }
            OnLevelLoaded?.Invoke(SceneManager.GetSceneByName(targetLevelName));
        }
        
        private static IEnumerator UnloadLevelRoutine(string currentSceneName) {
            BeforeLevelUnload?.Invoke(currentSceneName);
            yield return SceneManager.UnloadSceneAsync(currentSceneName);
            OnLevelUnloaded?.Invoke(currentSceneName);
        }
        
        private static IEnumerator LoadLoadingScene() {
            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            var loadingSceneObject = SceneManager.GetSceneByName(loadingScene);
            // because unity can be a little slow here, we wait until this has actually finished loading.
            yield return new WaitUntil(() => loadingSceneObject.isLoaded);
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


        public static void SetLoadingScene(string ls) {
            loadingScene = ls;
        }

        public static void ReloadActiveScene(Action<float> progress = null, float minLoadingTime = -1) {
            LoadLevelAsync(ActiveScene.name, progress, minLoadingTime);
        }
    }
}