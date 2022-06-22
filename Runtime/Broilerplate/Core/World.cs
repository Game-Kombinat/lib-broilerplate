using System;
using System.Collections.Generic;
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
        public WorldTime timeData;

        private UnityTicker unityTickerInstance;
        private TickManager tickManager;
        private GameMode gameMode;
        private readonly List<Actor> liveActors = new List<Actor>();
        private readonly List<WorldSubsystem> subsystems = new List<WorldSubsystem>();

        public TickManager TickManager => tickManager;
        public bool HasActors => liveActors.Count > 0;
        public int NumActors => liveActors.Count;

        /// <summary>
        /// Called first thing on a new tick.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void OnNewTick() {
            timeData.lastTick = timeData.thisTick;
            timeData.thisTick = DateTime.Now;
            timeData.deltaTime = (float)(timeData.thisTick - timeData.lastTick).TotalSeconds * timeData.timeDilation;
            timeData.timeSinceWorldBooted += timeData.deltaTime;
            // before ticks are called, process any new adds that have been made.
            tickManager.HandleScheduledTickAdds();
            // Handle ticks that want removing.
            tickManager.HandleScheduledTickRemovals();
        }

        public void ShutdownWorld() {
            for (int i = 0; i < liveActors.Count; i++) {
                UnregisterActor(liveActors[i--]);
            }
            Destroy(unityTickerInstance.gameObject);
        }

        public void BootWorld(GameMode gameModePrefab, List<WorldSubsystem> worldSubsystems) {
            timeData.timeDilation = 1;
            timeData.lastTick = timeData.thisTick = DateTime.Now;
            timeData.timeSinceWorldBooted = 0;
            timeData.deltaTime = 0;
            tickManager = new TickManager(this);

            
            var unityTicker = new GameObject("Unity Ticker");
            unityTickerInstance = unityTicker.AddComponent<UnityTicker>();
            unityTickerInstance.SetTickManager(tickManager);
            
            liveActors.Clear();
            liveActors.AddRange(FindObjectsOfType<Actor>());
            gameMode = gameModePrefab ? SpawnActor(gameModePrefab, Vector3.zero,Quaternion.identity) : SpawnActorOn<GameMode>(new GameObject("Default Game Mode"));
            if (worldSubsystems != null) {
                for (int i = 0; i < worldSubsystems.Count; i++) {
                    RegisterSubsystem(worldSubsystems[i]);
                }
            }
        }

        public void RegisterSubsystem(WorldSubsystem system) {
            var newSystem = Instantiate(system);
            subsystems.Add(newSystem);
            newSystem.SetWorld(this);
            newSystem.BeginPlay();
        }

        public void RegisterActor(Actor actor) {
            liveActors.Add(actor);
            actor.SetWorld(this);
            actor.BeginPlay();
        }

        public void BeginPlay() {
            for (int i = 0; i < liveActors.Count; i++) {
                if (liveActors[i].GetWorld() == this) {
                    // this was already registered, like a game mode for instance.
                    continue;
                }
                liveActors[i].SetWorld(this);
                liveActors[i].BeginPlay();
            }
        }

        public T SpawnActor<T>(T prefab) where T : Actor {
            var a = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            RegisterActor(a);

            return a;

        }
        
        public T SpawnActor<T>(T prefab, Vector3 position, Quaternion rotation) where T : Actor {
            var a = Instantiate(prefab, position, rotation);
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
        
        public GameObject SpawnActor(GameObject prefab) {
            return SpawnActor(prefab, Vector3.zero, Quaternion.identity);
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
            var t = a.GetComponent<Actor>();
            if (!t) {
                throw new ActorSpawnException($"Could not spawn actor from Prefab called {prefab.name} (use Instantiate() for non-actor prefabs)");
            }
            RegisterActor(t);

            return a;
        }
        
        public T SpawnActorOn<T>(GameObject targetGo) where T : Actor {
            targetGo = targetGo.transform.root.gameObject;
            Actor a = targetGo.AddComponent<T>();
            RegisterActor(a);

            return (T)a;
        }

        public void UnregisterActor(Actor actor) {
            UnregisterTickFunc(actor.ActorTick);
            liveActors.Remove(actor);

        }

        public void UnregisterTickFunc(TickFunc tickFunc) {
            tickManager.UnregisterTickFunc(tickFunc);
        }

        public void RegisterTickFunc(TickFunc tickFunc) {
            tickManager.RegisterTickFunc(tickFunc);
        }

        public void SpawnPlayer(PlayerInfo playerInfo) {
            var ps = FindActorOfType<PlayerStart>();
            if (ps) {
                gameMode.SpawnPlayer(playerInfo, ps.transform.position, ps.transform.rotation);
            }
            else {
                gameMode.SpawnPlayer(playerInfo, Vector3.zero, Quaternion.identity);
            }
            
        }

        private T FindActorOfType<T>() where T : Actor {
            for (int i = 0; i < liveActors.Count; i++) {
                var a = liveActors[i];
                if (a is T t) {
                    return t;
                }
            }

            return null;
        }

        public GameMode GetGameMode() {
            return gameMode;
        }

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