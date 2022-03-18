using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    /// <summary>
    /// This is the lowest level unit of Broilerplate.
    /// It gives you a singular entry point from which everything is started.
    /// </summary>
    public class GameInstance : ScriptableObject {
        private static GameInstance game;
        // todo: multi world setup: make dictionary scene => world hand handle that.
        private World world;
        private BroilerConfiguration configuration;
        public BroilerConfiguration Configuration => configuration;

        private List<PlayerInfo> localPlayers = new();
        
        public int NumLocalPlayers => localPlayers.Count;

        [RuntimeInitializeOnLoadMethod]
        public static void OnGameLoad() {
            // todo:
            // get additional level overrides from asset bundles
            
            Debug.Log("OnGameLoad");
            var config = BroilerConfiguration.GetConfiguration();
            if (game) {
                Debug.LogError("Calling GameLoad but GameInstance already exists. Will not override.");
                return;
            }
            game = Instantiate(config.GameInstanceType);
            game.InitiateGame(config);
        }

        public static GameInstance GetInstance() {
            return game;
        }

        /// <summary>
        /// Does some first-time bootstrapping processes.
        /// Does not need to be called again after level switch etc
        /// </summary>
        /// <param name="config"></param>
        public void InitiateGame(BroilerConfiguration config) {
            Debug.Log("Game Instance Initiate Game");
            DontDestroyOnLoad(this);
            LevelManager.OnLevelLoaded += OnLevelLoaded;
            LevelManager.BeforeLevelUnload += OnLevelUnloading;
            
            configuration = config;
            // we have to manually init the world at this point because when GameInstance awakes first time, 
            // the scene was already loaded.
            var scene = SceneManager.GetActiveScene();
            OnLevelLoaded(scene);
        }

        /// <summary>
        /// Creates the initial local player.
        /// As it would make no sense to have no player at all.
        /// There ought to be at least one.
        /// Except we're on a server build but lets cross that bridge when we get there.
        /// </summary>
        public PlayerInfo GetInitialLocalPlayer() {
            if (NumLocalPlayers > 0) {
                // this could return null but by design of this method, it shouldn't
                return GetPlayerOne();
            }

            var p = new PlayerInfo(0);
            localPlayers.Add(p);
            return p;
        }

        private PlayerInfo GetPlayerOne() {
            for (int i = 0; i < localPlayers.Count; i++) {
                if (localPlayers[i].PlayerId == 0) {
                    return localPlayers[i];
                }
            }
            return null;
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
            world = Instantiate(configuration.WorldType);
            world.BootWorld(configuration.GetGameModeFor(scene));
            world.SpawnPlayer(GetInitialLocalPlayer());
        }

        public World GetWorld() {
            return world;
        }
    }
}