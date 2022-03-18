using System;
using System.Collections.Generic;
using Broilerplate.Gameplay;
using Broilerplate.Gameplay.Input;
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

        private TickManager tickManager;
        private GameMode gameMode;
        private readonly List<Actor> liveActors = new();
        private readonly List<Actor> dirtyActors = new();
        private readonly List<Actor> actorsScheduledForDestroy = new();
        
        public TickManager TickManager => tickManager;

        /// <summary>
        /// Called first thing on a new tick.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void OnNewTick() {
            timeData.thisTick = DateTime.Now;
            timeData.deltaTime = (float)(timeData.thisTick - timeData.lastTick).TotalSeconds * timeData.timeDilation;
            timeData.timeSinceWorldBooted += timeData.deltaTime;
            // before ticks are called, process any new adds that have been made.
            tickManager.HandleScheduledTickAdds();
            // Handle ticks that want removing.
            tickManager.HandleScheduledTickRemovals();
        }

        public void ShutdownWorld() {
            // Generate a list of scheduled removals
            for (int i = 0; i < liveActors.Count; i++) {
                liveActors[i].DestroyActor();
            }
            // Force this list to be processed now
            HandleActorDestruction();
        }

        public void BootWorld(GameMode gameModePrefab) {
            Debug.Log("Booting ze world");
            timeData.timeDilation = 1;
            timeData.lastTick = timeData.thisTick = default;
            timeData.timeSinceWorldBooted = 0;
            timeData.deltaTime = 0;
            tickManager = new TickManager(this);

            var unityTicker = new GameObject("Unity Ticker");
            var ticker = unityTicker.AddComponent<UnityTicker>();
            ticker.SetTickManager(tickManager);
            
            liveActors.Clear();
            liveActors.AddRange(FindObjectsOfType<Actor>());
            gameMode = gameModePrefab ? SpawnActor(gameModePrefab, Vector3.zero,Quaternion.identity) : SpawnActorOn<GameMode>(new GameObject("Default Game Mode"));
            
            BeginPlay();
        }

        public void RegisterActor(Actor actor) {
            liveActors.Add(actor);
            actor.SetWorld(this);
            actor.BeginPlay();
        }

        public void MarkDirty(Actor actor) {
            dirtyActors.Add(actor);
        }

        public void ProcessDirtyActors() {
            for (int i = 0; i < dirtyActors.Count; ++i) {
                dirtyActors[i].ProcessComponentRemoval();
                // add more if actors get more stuff that needs this sort of handling.
                // or think of a more dynamic way to do this.
            }
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

        public T SpawnActor<T>(T prefab, Vector3 position, Quaternion rotation) where T : Actor {
            var a = Instantiate(prefab, position, rotation);
            RegisterActor(a);

            return a;

        }
        
        public T SpawnActorOn<T>(GameObject targetGo) where T : Actor {
            targetGo = targetGo.transform.root.gameObject;
            Actor a = targetGo.AddComponent<T>();
            RegisterActor(a);

            return (T)a;
        }

        public void DestroyActor(Actor actor) {
            actorsScheduledForDestroy.Add(actor);
            
        }

        public void UnregisterTickFunc(TickFunc tickFunc) {
            tickManager.UnregisterTickFunc(tickFunc);
        }

        public void RegisterTickFunc(TickFunc tickFunc) {
            tickManager.RegisterTickFunc(tickFunc);
        }

        private void HandleActorDestruction() {
            for (int i = 0; i < actorsScheduledForDestroy.Count; i++) {
                var actor = actorsScheduledForDestroy[i];
                liveActors.Remove(actor);
                UnregisterTickFunc(actor.ActorTick);
                Destroy(actor.gameObject);
            }
        }

        public void SpawnPlayer(PlayerInfo playerInfo) {
            // todo: find spawn point type and spawn there!
            gameMode.SpawnPlayer(playerInfo, Vector3.zero, Quaternion.identity);
        }

        public GameMode GetGameMode() {
            return gameMode;
        }
    }
}