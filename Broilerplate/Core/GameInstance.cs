using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    /// <summary>
    /// This is the lowest level unit of Broilerplate.
    /// It gives you a singular entry point from which everything is started.
    /// </summary>
    public class GameInstance : ScriptableObject {
        private static GameInstance game;
        private World world;
        
        [RuntimeInitializeOnLoadMethod]
        public static void OnGameLoad() {
            // todo:
            // Determine which game instance class to use and load it.
            // this probably happens only once when runtime is started (otherwise add a guard that makes it so)
            // then determine which world class to use.
            // then, lastly, determine which game mode should be loaded into the world.
            // Should be in a kind of global configuration asset that can be expanded at runtime.
            // (I'm thinking asset bundles - those should be pre-processed in here before game instance is initiated.)
            
            Debug.Log("OnGameLoad");
            game = CreateInstance<GameInstance>();
        }

        public static GameInstance GetInstance() {
            return game;
        }

        protected virtual void Awake() {
            DontDestroyOnLoad(this);
            Debug.Log("Game instance Awake");
            if (game) {
                Destroy(this);
                Debug.LogError("Attempted to start a second game instance. This is illegal!");
                return;
            }

            LevelManager.OnLevelLoaded += OnLevelLoaded;
            LevelManager.BeforeLevelUnload += OnLevelUnloading;
            
            // we have to manually init the world at this point because when GameInstance awakes first time, 
            // the scene was already loaded.
            OnLevelLoaded(SceneManager.GetActiveScene());
        }

        private void OnLevelUnloading(Scene unloadingScene) {
            // get world for scene, call some handling, destroy world.
            // unity does not do this because world isn't under the scene root.
            world.ShutdownWorld();
            world = null;
        }

        private void OnLevelLoaded(Scene scene) {
            // create a new world from project config
            // create a mapping scene => world.
            // call RegisterActors() on world to kick off managed BeginPlay() callbacks.
            world = CreateInstance<World>();
            world.BootWorld();
            world.BeginPlay();
        }

        public World GetWorld() {
            return world;
        }
    }
}