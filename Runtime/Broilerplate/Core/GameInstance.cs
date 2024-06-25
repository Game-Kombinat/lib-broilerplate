using System.Collections.Generic;
using Broilerplate.Core.Exceptions;
using Broilerplate.Core.Subsystems;
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
    [CreateAssetMenu(menuName = "Broilerplate/New Game Instance", fileName = "Game Instance")]
    public class GameInstance : ScriptableObject {
        /// <summary>
        /// This is created once when the game boots first-time and remains in memory
        /// until the game is shut down.
        /// </summary>
        private static GameInstance gameInstance;
        
        [SerializeField]
        private List<GameSubsystem> gameSubsystems;

        private List<GameSubsystem> gameSubsystemInstances = new();
        
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
            gameInstance.OnInitiate();
            gameInstance.InitiateGame(broilerConfigFile);
        }

        protected virtual void OnInitiate() { }

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
        public void InitiateGame(BroilerConfiguration broilerConfiguration) {
            DontDestroyOnLoad(this);
            LevelManager.SetLoadingScene(broilerConfiguration.LoadingScene);
            LevelManager.OnLevelLoaded += OnLevelLoaded;
            LevelManager.BeforeLevelUnload += OnLevelUnloading;

            GameInstanceConfiguration = broilerConfiguration;

            RegisterGameSubsystems();
            // we have to manually init the world at this point because when GameInstance awakes first time, 
            // the scene was already loaded.
            var scene = SceneManager.GetActiveScene();

            if (scene.path == GameInstanceConfiguration.LoadingScene?.ScenePath) {
                LevelManager.LoadLevelAsync(GameInstanceConfiguration.StartupScene, LevelLoadProgress.OnProgress);
            }
            else {
                OnLevelLoaded(scene);
            }
        }

        protected virtual void OnDestroy() {
            UnregisterGameSubsystems();
        }

        private void HandleSubsystemsWorldBooted(World w) {
            for (int i = 0; i < gameSubsystemInstances.Count; i++) {
                var sys = gameSubsystemInstances[i];
                sys.OnWorldSpawned(w);
            }
        }

        private void HandleSubsystemsWorldQuit(World w, string nextLevel) {
            for (int i = 0; i < gameSubsystemInstances.Count; i++) {
                var sys = gameSubsystemInstances[i];
                // Check for null in case Unity already cleaned up the backrooms. Shouldn't happen right here though.
                // But just to be sure. Doesn't hurt anybody at this point.
                if (sys) {
                    sys.OnWorldDespawning(w, nextLevel);
                }
            }
        }

        private void RegisterGameSubsystems() {
            UnregisterGameSubsystems();
            for (int i = 0; i < gameSubsystems.Count; i++) {
                var sys = Instantiate(gameSubsystems[i]);
                gameSubsystemInstances.Add(sys);
            }
            
            // Sort for LateBeginPlay, then go.
            gameSubsystemInstances.Sort((a, b) => a.InitialisationPriority.CompareTo(b.InitialisationPriority));
            for (int i = 0; i < gameSubsystemInstances.Count; i++) {
                var sys = gameSubsystemInstances[i];
                sys.BeginPlay();
                sys.LateBeginPlay();
            }
        }

        private void UnregisterGameSubsystems() {
            for (int i = 0; i < gameSubsystemInstances.Count; i++) {
                var sys = gameSubsystemInstances[i];
                if (sys) {
                    // They might be null already because Unity was faster.
                    // In which case ignore them.
                    sys.OnGameEnds();
                }
            }
            
            gameSubsystemInstances.Clear();
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
        /// <param name="nextLevel"></param>
        private void OnLevelUnloading(Scene unloadingScene, string nextLevel) {
            HandleSubsystemsWorldQuit(world, nextLevel);
            
            // get world for scene, call some handling, destroy world.
            // unity does not do this because world isn't under the scene root.
            if (world) {
                world.ShutdownWorld();
            }
            Destroy(world);
            world = null;
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
            HandleSubsystemsWorldBooted(world);
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
            world.BootWorld(gmPrefab, GameInstanceConfiguration.WorldSubsystems, scene.name);
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

        public T GetSubsystem<T>() where T : GameSubsystem {
            for (int i = 0; i < gameSubsystemInstances.Count; i++) {
                var sys = gameSubsystemInstances[i];
                if (sys is T subsystem) {
                    return subsystem;
                }
            }

            return null;
        }
        
        public List<T> GetSubsystems<T>() {
            List<T> result = new();
            
            for (int i = 0; i < gameSubsystemInstances.Count; i++) {
                var sys = gameSubsystemInstances[i];
                
                if (sys is T t) {
                    result.Add(t);
                }
            }
            
            return result;
        }
    }
}