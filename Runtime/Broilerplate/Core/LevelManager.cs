using System;
using System.Collections;
using Broilerplate.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    /// <summary>
    /// Wraps around the Unity SceneManager and exposes a couple
    /// extra callbacks throughout the scene loading / unloading process,
    /// for greater control over said process
    /// </summary>
    public static class LevelManager {
        /// <summary>
        /// Callback before a new level is loaded.
        /// This has a string as parameter (the level name) as the unity
        /// scene cannot be retrieved at this point as it is not loaded yet.
        /// </summary>
        public static event Action<string> BeforeLevelLoad;
        
        /// <summary>
        /// Callback for when a level is done loading and the world can be instantiated into it.
        /// At this point the loaded level has been set as active.
        /// </summary>
        public static event Action<Scene> OnLevelLoaded;
        
        /// <summary>
        /// Callback for when a level has been unloaded.
        /// It has a string as parameter (the name of the unloaded level) because at this point,
        /// we cannot retrieve the scene object from scene manager anymore.
        /// </summary>
        public static event Action<string> OnLevelUnloaded;
        
        /// <summary>
        /// Callback that's called right before a new level is about to be loaded.
        /// </summary>
        public static event Action<Scene> BeforeLevelUnload;

        /// <summary>
        /// Name of the loading scene if there is one
        /// </summary>
        private static string loadingScene;
        
        /// <summary>
        /// Shortcut to get the active scene.
        /// </summary>
        public static Scene ActiveScene => SceneManager.GetActiveScene();

        /// <summary>
        /// Flag to indicate whether we're currently loading a scene or not.
        /// </summary>
        private static bool loadingInProgress;


        /// <summary>
        /// The public API of this. Handles everything that needs handling when loading a new level.
        /// That includes putting the loading scene in, cleaning garbage out of memory and things of that nature.
        /// </summary>
        /// <param name="levelName"></param>
        /// <param name="progress"></param>
        /// <param name="minimumLoadingTime"></param>
        public static void LoadLevelAsync(string levelName, Action<float> progress = null, float minimumLoadingTime = -1) {
            if (loadingInProgress) {
                Debug.LogWarning($"Attempting to load level {levelName} while another is currently being loaded. Aborting this.");
                return;
            }
            if (minimumLoadingTime < 0) {
                minimumLoadingTime = GameInstance.GetInstance().GameInstanceConfiguration.DefaultMinimumLoadingTime;
            }
            CoroutineJobs.StartJob(DoLoadLevelAsync(levelName, minimumLoadingTime, progress), true);
        }
        
        /// <summary>
        /// Handles the loading of a new level. This contains the actual logic described in LoadLevelAsync.
        /// </summary>
        /// <param name="targetLevelName"></param>
        /// <param name="fakeLoadingTime"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private static IEnumerator DoLoadLevelAsync(string targetLevelName, float fakeLoadingTime, Action<float> progress) {
            loadingInProgress = true;
            var currentSceneObject = SceneManager.GetActiveScene();
            string currentSceneName = currentSceneObject.name;
            
            if (!string.IsNullOrEmpty(loadingScene)) {
                bool currentIsLoadingScene = true;
                // if we have a loading scene configured, load this in first
                if (loadingScene != currentSceneName) {
                    currentIsLoadingScene = false;
                    yield return LoadLoadingScene();
                }

                if (!currentIsLoadingScene) {
                    // unload currently loaded level if we didn't start from the loading scene
                    yield return UnloadLevelRoutine(currentSceneObject);
                }
                // Now load the new scene and get rid of the loading scene if necessary
                yield return LoadLevelRoutine(targetLevelName, fakeLoadingTime, progress, !currentIsLoadingScene);
            }
            else {
                // we have to manually call the unload callback for the current scene here because
                // on this code path we don't explicitly unload it. and we can't because unity doesn't
                // allow no scene to be loaded. (Which I think is fine)
                BeforeLevelUnload?.Invoke(ActiveScene);
                string unloadedLevel = ActiveScene.name;
                yield return LoadLevelRoutine(targetLevelName, fakeLoadingTime, progress, false);
                OnLevelUnloaded?.Invoke(unloadedLevel); // This is not necessarily the correct location but it's the best we can get
            }
            loadingInProgress = false;
        }

        /// <summary>
        /// The standard routine to load a level.
        /// </summary>
        /// <param name="targetLevelName"></param>
        /// <param name="fakeLoadingTime"></param>
        /// <param name="progress"></param>
        /// <param name="unloadLoadingScene"></param>
        /// <returns></returns>
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

            var activeScene = SceneManager.GetSceneByName(targetLevelName);
            SceneManager.SetActiveScene(activeScene);
            OnLevelLoaded?.Invoke(activeScene);
        }
        
        /// <summary>
        /// The standard routine to unload a level
        /// </summary>
        /// <param name="currentScene"></param>
        /// <returns></returns>
        private static IEnumerator UnloadLevelRoutine(Scene currentScene) {
            string sceneName = currentScene.name;
            BeforeLevelUnload?.Invoke(currentScene);
            yield return SceneManager.UnloadSceneAsync(currentScene);
            OnLevelUnloaded?.Invoke(sceneName);
        }
        
        /// <summary>
        /// The standard routine to load the loading scene.
        /// </summary>
        /// <returns></returns>
        private static IEnumerator LoadLoadingScene() {
            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            var loadingSceneObject = SceneManager.GetSceneByName(loadingScene);
            // because unity can be a little slow here, we wait until this has actually finished loading.
            yield return new WaitUntil(() => loadingSceneObject.isLoaded);
        }

        /// <summary>
        /// The loop that waits for the loading of a level to finish.
        /// This can put in a fake loading time as well.
        /// It sounds counter-intuitive to do this but trust me. There are use cases for that.
        /// </summary>
        /// <param name="fakeLoadingTime"></param>
        /// <param name="progress"></param>
        /// <param name="loadNewScene"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Sets the loading scene name from broiler configuration.
        /// 
        /// </summary>
        /// <param name="ls"></param>
        public static void SetLoadingScene(string ls) {
            loadingScene = ls;
        }

        /// <summary>
        /// Shortcut to reload the active scene. This will also re-bootstrap the world and
        /// everything that comes with it.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="minLoadingTime"></param>
        public static void ReloadActiveScene(Action<float> progress = null, float minLoadingTime = -1) {
            LoadLevelAsync(ActiveScene.name, progress, minLoadingTime);
        }
    }
}