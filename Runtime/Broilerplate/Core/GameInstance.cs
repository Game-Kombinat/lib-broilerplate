﻿using System.Collections.Generic;
using Broilerplate.Core.Exceptions;
using Broilerplate.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    /// <summary>
    /// This is the lowest level unit of Broilerplate.
    /// It gives you a singular entry point from which everything is started.
    /// From here it's possible to access the currently running world and consequently
    /// the runtime API of Broilerplate.
    /// </summary>
    public class GameInstance : ScriptableObject {

        /// <summary>
        /// This is created once when the game boots first-time and remains in memory
        /// until the game is shut down.
        /// </summary>
        private static GameInstance gameInstance;
        
        /// <summary>
        /// Currently loaded world object.
        /// </summary>
        // todo: multi world setup: make dictionary scene => world hand handle that.
        private World world;

        /// <summary>
        /// The configuration file we source our game modes from.
        /// </summary>
        public BroilerConfiguration GameInstanceConfiguration { get; private set; }

        /// <summary>
        /// List of players in the given world.
        /// Currently this isn't tied into anything. Long-term plan is to
        /// tie it into the input management to automate that.
        /// </summary>
        // Will probably be used for multiplayer concept, right now its gonna be only 1.
        private List<PlayerInfo> localPlayers = new List<PlayerInfo>();
        
        public int TotalLocalPlayers => localPlayers.Count;

        /// <summary>
        /// This is the entry-point for broilerplate.
        /// It creates the game instance and handles first time boot after the first scene was loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void OnGameLoad() 
        {
            // todo:
            // get additional level overrides from asset bundles
            
            var broilerConfigFile = BroilerConfiguration.GetConfiguration();

            if (gameInstance) {
                Debug.LogError("Calling GameLoad but GameInstance already exists. Will not override.");
                return;
            }
            gameInstance = Instantiate(broilerConfigFile.GameInstanceType);
            gameInstance.InitiateGame(broilerConfigFile);
        }

        /// <summary>
        /// Static to get the game instance.
        /// </summary>
        /// <returns></returns>
        public static GameInstance GetInstance() {
            return gameInstance;
        }

        /// <summary>
        /// Does some first-time bootstrapping processes.
        /// Does not need to be called again after level switch etc
        /// </summary>
        /// <param name="broilerConfiguration"></param>
        public void InitiateGame(BroilerConfiguration broilerConfiguration) 
        {
            DontDestroyOnLoad(this);
            LevelManager.SetLoadingScene(broilerConfiguration.LoadingScene);
            LevelManager.OnLevelLoaded += OnLevelLoaded;
            LevelManager.BeforeLevelUnload += OnLevelUnloading;
            
            GameInstanceConfiguration = broilerConfiguration;
            // we have to manually init the world at this point because when GameInstance awakes first time, 
            // the scene was already loaded.
            var scene = SceneManager.GetActiveScene();
            
            if (scene.path == GameInstanceConfiguration.LoadingScene?.ScenePath)
            {
                LevelManager.LoadLevelAsync(GameInstanceConfiguration.StartupScene, LevelLoadProgress.OnProgress);
            }
            else
            {
                OnLevelLoaded(scene);
            }
            
        }

        /// <summary>
        /// Creates the initial local player.
        /// As it would make no sense to have no player at all.
        /// There ought to be at least one.
        /// Except we're on a server build but lets cross that bridge when we get there.
        /// </summary>
        public PlayerInfo GetInitialLocalPlayer() {
            if (TotalLocalPlayers > 0) {
                // this could return null but by design of this method, it shouldn't
                return GetPlayerOne();
            }

            var p = new PlayerInfo(0);
            localPlayers.Add(p);
            return p;
        }

        /// <summary>
        /// Retrieves the PlayerInfo with ID 0.
        /// </summary>
        /// <returns></returns>
        private PlayerInfo GetPlayerOne() {
            for (int i = 0; i < localPlayers.Count; i++) {
                if (localPlayers[i].PlayerId == 0) {
                    return localPlayers[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Callback when a level is unloaded.
        /// Calls ShutdownWorld and cleans up.
        /// </summary>
        /// <param name="unloadingScene"></param>
        private void OnLevelUnloading(Scene unloadingScene) {
            // get world for scene, call some handling, destroy world.
            // unity does not do this because world isn't under the scene root.
            if (world) {
                world.ShutdownWorld();
                world = null;
            }
        }

        /// <summary>
        /// Callback when a level is loaded.
        /// Calls BootstrapWorld when auto-bootstrapping is enabled.
        /// </summary>
        /// <param name="scene"></param>
        private void OnLevelLoaded(Scene scene) {
            if (scene.path == GameInstanceConfiguration.LoadingScene?.ScenePath) {
                return;
            }
            // create a new world from project config
            // create a mapping scene => world.
            // call RegisterActors() on world to kick off managed BeginPlay() callbacks.
            if (GameInstanceConfiguration.AutoBootstrapWorld) {
                BootstrapWorldForLevel(scene);
            }
            
        }

        /// <summary>
        /// Handles the process of bootstrapping the world and therefore
        /// the whole broilerplate runtime API.
        /// That's actors, subsystems, game modes, player pawn spawning.
        /// </summary>
        /// <param name="scene"></param>
        public void BootstrapWorldForLevel(Scene scene) {
            var gmPrefab = GameInstanceConfiguration.GetGameModeFor(scene);
            
            world = Instantiate(GameInstanceConfiguration.WorldType);
            world.name = $"{world.GetType().Name} for {scene.name}";
            world.BootWorld(gmPrefab, GameInstanceConfiguration.WorldSubsystems);
            world.SpawnPlayer(GetInitialLocalPlayer());
            // Finalise bootstrapping game mode with player controller having been initialised.
            // BeginPlay for GameMode has been called way before this point when the game world has spawned it.
            var gm = world.GetGameMode();
            if (!gm) {
                throw new GameException($"Spawned world is null. Prefab we tried to spawn from is {(gmPrefab != null ? gmPrefab.name : "null")}");
            }
            
            gm.LateBeginPlay(); // Manually invoke this before end of frame so GameMode is fully there before actors get their BeginPlay routines called
            world.BeginPlay();
        }

        /// <summary>
        /// Get the currently loaded world.
        /// </summary>
        /// <returns></returns>
        public World GetWorld() {
            return world;
        }
    }
}