using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Core {
    /// <summary>
    /// This is the lowest level unit of Broilerplate.
    /// It gives you a singular entry point from which everything is started.
    /// </summary>
    public class GameInstance : ScriptableObject {

        private static GameInstance gameInstance;
        // todo: multi world setup: make dictionary scene => world hand handle that.
        private World world;

        public BroilerConfiguration GameInstanceConfiguration { get; private set; }

        // Will probably be used for multiplayer concept, right now its gonna be only 1.
        private List<PlayerInfo> localPlayers = new List<PlayerInfo>();
        
        public int TotalLocalPlayers => localPlayers.Count;

        [RuntimeInitializeOnLoadMethod]
        public static void OnGameLoad() 
        {
            // todo:
            // get additional level overrides from asset bundles
            
            Debug.Log("OnGameLoad, It should be loaded only once");
            var broilerConfigFile = BroilerConfiguration.GetConfiguration();

            if (gameInstance) {
                Debug.LogError("Calling GameLoad but GameInstance already exists. Will not override.");
                return;
            }
            gameInstance = Instantiate(broilerConfigFile.GameInstanceType);
            gameInstance.InitiateGame(broilerConfigFile);
        }

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
            LevelManager.OnLevelLoaded += OnLevelLoaded;
            LevelManager.BeforeLevelUnload += OnLevelUnloading;
            
            GameInstanceConfiguration = broilerConfiguration;
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
            if (TotalLocalPlayers > 0) {
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
            world = Instantiate(GameInstanceConfiguration.WorldType);
            world.BootWorld(GameInstanceConfiguration.GetGameModeFor(scene), GameInstanceConfiguration.WorldSubsystems);
            world.SpawnPlayer(GetInitialLocalPlayer());
            // Finalise bootstrapping game mode with player controller having been initialised.
            // BeginPlay for GameMode has been called way before this point when the game world has spawned it.
            world.GetGameMode().LateBeginPlay();
            world.BeginPlay();
        }

        public World GetWorld() {
            return world;
        }
    }
}