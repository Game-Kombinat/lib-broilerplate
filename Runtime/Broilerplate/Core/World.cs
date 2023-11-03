using System;
using System.Collections.Generic;
using System.Linq;
using Broilerplate.Core.Components;
using Broilerplate.Core.Exceptions;
using Broilerplate.Core.Subsystems;
using Broilerplate.Gameplay;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Core {
    /// <summary>
    /// Transient type that contains everything related to a level.
    /// That can be but is not limited to:
    /// * The current GameMode
    /// * List of actors
    /// This may, at some point in time, become part of a tick loop implementation.
    /// As we know. Calling custom ticks is easier on the cpu than letting unity call Update().
    /// And we gain lots of control.
    /// </summary>
    public class World : ScriptableObject {

        private UnityTicker unityTickerInstance;
        private TickManager tickManager;
        protected GameMode gameMode;
        protected readonly List<Actor> liveActors = new List<Actor>();
        protected readonly List<WorldSubsystem> subsystems = new List<WorldSubsystem>();

        public TickManager TickManager => tickManager;
        public bool HasActors => liveActors.Count > 0;
        public int NumActors => liveActors.Count;

        /// <summary>
        /// Called first thing on a new tick.
        /// </summary>
        public void HandleTickChanges() {
            // Handle ticks that want removing.
            // handle removes first because of possible re-registers.
            tickManager.HandleScheduledTickRemovals();
            // before ticks are called, process any new adds that have been made.
            tickManager.HandleScheduledTickAdds();
        }

        /// <summary>
        /// Called when a level is about to get unloaded.
        /// Cleans up all actors and subsystems to give them a chance to process the situation.
        /// </summary>
        public virtual void ShutdownWorld() {
            Application.quitting -= ShutdownWorld;
            for (int i = 0; i < liveActors.Count; i++) {
                UnregisterActor(liveActors[i--]);
            }
            ClearSubSystems();
            if (unityTickerInstance) {
                Destroy(unityTickerInstance.gameObject);
            }
        }

        /// <summary>
        /// Called when a new level is being loaded or the game is loaded the first time.
        /// Sets up the world for the loaded level and spawns the game mode and subsystems.
        /// </summary>
        /// <param name="gameModePrefab"></param>
        /// <param name="worldSubsystems"></param>
        public virtual void BootWorld(GameMode gameModePrefab, List<WorldSubsystem> worldSubsystems) {
            tickManager = new TickManager(this);

            var unityTicker = new GameObject("Unity Ticker");
            unityTickerInstance = unityTicker.AddComponent<UnityTicker>();
            unityTickerInstance.SetTickManager(tickManager);
            
            if (worldSubsystems != null) {
                for (int i = 0; i < worldSubsystems.Count; i++) {
                    RegisterSubsystem(worldSubsystems[i]);
                }
            }
            
            liveActors.Clear();
            liveActors.AddRange(FindObjectsOfType<Actor>(true));
            // A world implementation might have overridden the creation of the game mode, so check if it's already there.
            if (!gameMode) {
                gameMode = gameModePrefab ? SpawnActor(gameModePrefab, Vector3.zero,Quaternion.identity) : SpawnActorOn<GameMode>(new GameObject("Default Game Mode"));
            }
            unityTicker.transform.SetParent(gameMode.transform);
        }

        /// <summary>
        /// Register a Subsytem to the world. Spawns it and handles the BeginPlay routines.
        /// </summary>
        /// <param name="system"></param>
        public virtual void RegisterSubsystem(WorldSubsystem system) {
            var newSystem = Instantiate(system);
            subsystems.Add(newSystem);
            newSystem.SetWorld(this);
            ScheduleBeginPlay(newSystem);
        }

        /// <summary>
        /// Register an actor with the world.
        /// </summary>
        /// <param name="actor"></param>
        protected virtual void RegisterActor(Actor actor) {
            liveActors.Add(actor);
            actor.SetWorld(this);
            ScheduleBeginPlay(actor);
        }

        /// <summary>
        /// Worlds BeginPlay will call BeginPlay on actors and schedule their LateBeginPlay calls for the end of frame.
        /// </summary>
        public virtual void BeginPlay() {
            Application.quitting += ShutdownWorld;
            for (int i = 0; i < liveActors.Count; i++) {
                if (liveActors[i].GetWorld() == this) {
                    // this was already registered, like a game mode for instance.
                    continue;
                }
                liveActors[i].SetWorld(this);
                ScheduleBeginPlay(liveActors[i]);
            }
        }

        /// <summary>
        /// Clear out all currently registered subsystems.
        /// </summary>
        private void ClearSubSystems() {
            for (int i = 0; i < subsystems.Count; i++) {
                Destroy(subsystems[i]);
            }
            
            subsystems.Clear();
        }

        public T SpawnActor<T>(string objectName) where T : Actor {
            var obj = new GameObject(objectName);
            return SpawnActorOn<T>(obj);
        }

        public T SpawnActor<T>(T prefab) where T : Actor {
            return SpawnActor(prefab, Vector3.zero, Quaternion.identity);
        }
        
        public T SpawnActor<T>(T prefab, Vector3 position, Quaternion rotation) where T : Actor {
            var a = Instantiate(prefab, position, rotation);
            RegisterActor(a);
            return a;
        }
        
        public T SpawnActor<T>(T prefab, Transform parent) where T : Actor {
            var a = Instantiate(prefab, parent);
            RegisterActor(a);
            return a;
        }
        
        public T SpawnActor<T>(GameObject prefab) where T : Actor {
            return SpawnActor<T>(prefab, Vector3.zero, Quaternion.identity);
        }
        
        public T SpawnActor<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Actor {
            var a = Instantiate(prefab, position, rotation);
            var t = a.GetComponent<T>();
            if (!t) {
                throw new ActorSpawnException($"Could not spawn actor of type {typeof(T)} from Prefab called {prefab.name}");
            }
            RegisterActor(t);
            return t;
        }
        
        public T SpawnActor<T>(GameObject prefab, Transform parent) where T : Actor {
            return SpawnActor<T>(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        
        public T SpawnActor<T>(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Actor {
            var a = Instantiate(prefab, parent);
            a.transform.localPosition = position;
            a.transform.localRotation = rotation;
            var t = a.GetComponent<T>();
            if (!t) {
                throw new ActorSpawnException($"Could not spawn actor of type {typeof(T)} from Prefab called {prefab.name}");
            }
            RegisterActor(t);
            return t;
        }
        
        public GameObject SpawnActor(GameObject prefab) {
            return SpawnActor(prefab, prefab.transform.position, prefab.transform.rotation);
        }
        
        /// <summary>
        /// Spawns whatever actor is on the given prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <exception cref="ActorSpawnException"></exception>
        public GameObject SpawnActor(GameObject prefab, Vector3 position, Quaternion rotation) {
            var a = Instantiate(prefab, position, rotation);
            var actors = a.GetComponentsInChildren<Actor>(true);
            if (actors == null || actors.Length == 0)
            {
                return a;
            }

            for (int i = 0; i < actors.Length; ++i)
            {
                RegisterActor(actors[i]);
            }
            
            return a;
        }
        
        public GameObject SpawnActor(GameObject prefab, Transform parent) {
            return SpawnActor(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        
        public GameObject SpawnActor(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation) {
            var a = Instantiate(prefab, parent);
            a.transform.localPosition = position;
            a.transform.localRotation = rotation;
            
            var actors = a.GetComponentsInChildren<Actor>(true);
            if (actors == null || actors.Length == 0)
            {
                return a;
            }

            for (int i = 0; i < actors.Length; ++i)
            {
                RegisterActor(actors[i]);
            }
            
            return a;
        }
        
        public T SpawnActorOn<T>(GameObject targetGo) where T : Actor {
            targetGo = targetGo.transform.gameObject;
            Actor a = targetGo.AddComponent<T>();
            RegisterActor(a);

            return (T)a;
        }

        /// <summary>
        /// Unregister an actor from the world.
        /// Will also unregister their tickfuncs.
        /// It is usually called when an actor dies / is destroyed.
        /// Doing it during the actors lifetime will effectively turn it into a normal MonoBehaviour.
        /// </summary>
        /// <param name="actor"></param>
        public void UnregisterActor(Actor actor) {
            UnregisterTickFunc(actor.ActorTick);
            liveActors.Remove(actor);

        }

        /// <summary>
        /// Unregister the given tickfunc from the tick manager of this world.
        /// </summary>
        /// <param name="tickFunc"></param>
        public void UnregisterTickFunc(TickFunc tickFunc) {
            tickManager.UnregisterTickFunc(tickFunc);
        }

        /// <summary>
        /// Register the given tickfunc to the tick manager of this world.
        /// </summary>
        /// <param name="tickFunc"></param>
        public void RegisterTickFunc(TickFunc tickFunc) {
            tickManager.RegisterTickFunc(tickFunc);
        }
        
        /// <summary>
        /// Calls BeginPlay on the given IInitialise and schedules it for
        /// LateBeginPlay at the end of the frame.
        /// </summary>
        /// <param name="i"></param>
        public void ScheduleBeginPlay(IInitialise i) {
            tickManager.ScheduleBeginPlay(i);
        }

        /// <summary>
        /// Spawns the players pawn as defined in the GameMode.
        /// </summary>
        /// <param name="playerInfo"></param>
        public void SpawnPlayer(PlayerInfo playerInfo) {
            var ps = FindActorOfType<PlayerStart>();
            if (ps) {
                gameMode.SpawnPlayer(playerInfo, ps.transform.position, ps.transform.rotation);
            }
            else {
                gameMode.SpawnPlayer(playerInfo, Vector3.zero, Quaternion.identity);
            }
            
        }

        /// <summary>
        /// Finds an actor by the given type.
        /// This is a bit like Object.FindObjectsOfType but faster since
        /// the pool of objects it needs to scan is a whooooooole lot smaller. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindActorOfType<T>() where T : Actor {
            for (int i = 0; i < liveActors.Count; i++) {
                var a = liveActors[i];
                if (a is T t) {
                    return t;
                }
            }

            return null;
        }

        public List<T> FindActorsOfType<T>() where T : Actor {
            return liveActors.OfType<T>().ToList(); // :O
        }

        /// <summary>
        /// Get a reference to the GameMode that is running in this world.
        /// </summary>
        /// <returns></returns>
        public GameMode GetGameMode() {
            return gameMode;
        }
        
        /// <summary>
        /// Generic version of GetGameMode. Will attempt to direct-cast the gameMode to T.
        /// Throws InvalidCastException if and when that fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetGameMode<T>() where T : GameMode {
            return (T)gameMode;
        }

        /// <summary>
        /// Returns the first found subsystem of the given type T.
        /// Returns null if there is no such subsystem registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSubsystem<T>() where T : WorldSubsystem {
            for (int i = 0; i < subsystems.Count; i++) {
                var s = subsystems[i];
                if (s is T castSystem) {
                    return castSystem;
                }
            }

            return null;
        }
    }
}