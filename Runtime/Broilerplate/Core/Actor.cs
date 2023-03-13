using System;
using System.Collections.Generic;
using Broilerplate.Core.Components;
using Broilerplate.Ticking;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Broilerplate.Core {
    /// <summary>
    /// Base class for all gameplay relevant things that are in the world.
    /// Every prefab needs to have an actor on its root object.
    /// </summary>
    [DisallowMultipleComponent]
    public class Actor : MonoBehaviour, ITickable, IThing, IInitialise {
        protected World world;


        [SerializeField] protected TickFunc actorTick = new TickFunc();

        public int NumComponents => registeredComponents.Count;
        
        public Actor ParentActor { get; private set; }

        public bool HasTickFunc => actorTick != null;

        public bool HasBegunPlaying { get; private set; } = false;
        
        public bool HadLateBeginPlay { get; private set; } = false;

        private readonly List<ActorComponent> registeredComponents = new List<ActorComponent>();


        public TickFunc ActorTick => actorTick;

        protected virtual void Awake() {
            Reset();
        }

        private void OnTransformParentChanged() {
            if (!transform.parent) {
                ParentActor = null;
                return;
            }

            ParentActor = transform.parent.GetComponentInParent<Actor>();
        }


        /// <summary>
        /// Called when this actor is spawned.
        /// This method is similar to Awake() but within the execution order the broiler.
        /// Also works on actors already in the scene.
        /// Be sure to call the super function to ensure everything is working!
        /// </summary>
        public virtual void BeginPlay() {
            // register tick function to world.
            if (HasTickFunc && actorTick.CanEverTick) {
                actorTick.SetTickTarget(this);
                world.RegisterTickFunc(actorTick);
            }

            // why do this? So we know the owner actor has been initialised with BeginPlay before its components.
            // Now we get a known execution order.
            for (int i = 0; i < registeredComponents.Count; ++i) {
                var comp = registeredComponents[i];
                world.ScheduleBeginPlay(comp);
            }

            // mark as ready so when components get added now, during runtime,
            // they get BeginPlay called right away.
            HasBegunPlaying = true;
        }

        /// <summary>
        /// Called at the end of the frame this actor had been spawned in.
        /// This method is similar Start() but functions within the execution order of the broiler.
        /// </summary>
        public virtual void LateBeginPlay() {
            HadLateBeginPlay = true;
        }

        /// <summary>
        /// Calling this will keep the tickfunc registered but its tick will not be called.
        /// This therefore does not save time on tickfunc interation but it saves time
        /// when executing the tick and when inserting / sorting the tick into the tick list.
        /// </summary>
        /// <param name="shouldTick"></param>
        public void SetEnableTick(bool shouldTick) {
            if (!actorTick.CanEverTick) {
                Debug.LogWarning($"Attempted to change tick on actor that never ticks: {name}");
                return;
            }

            actorTick.SetEnableTick(shouldTick);
        }

        public void UnregisterTickFunc() {
            if (world) {
                world.UnregisterTickFunc(actorTick);
            }
        }

        public virtual void OnEnableTick() {
            
        }

        public virtual void OnDisableTick() {
            
        }

        /// <summary>
        /// Called before BeginPlay either when actor is spawned or fetched from existing world.
        /// </summary>
        /// <param name="inWorld"></param>
        public void SetWorld(World inWorld) {
            world = inWorld;
        }

        public void SetGameObjectActive(bool active) {
            gameObject.SetActive(active);
            for (int i = 0; i < registeredComponents.Count; i++) {
                registeredComponents[i].OnGameObjectStatusChange(active);
            }
        }

        public T GetGameComponent<T>() where T : ActorComponent {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                if (registeredComponents[i] is T) {
                    return (T)registeredComponents[i];
                }
            }

            return null;
        }

        public int GetGameComponents<T>(T[] cache) where T : ActorComponent {
            int hits = 0;
            for (int i = 0; i < cache.Length; ++i) {
                if (registeredComponents.Count > i && registeredComponents[i] is T comp) {
                    cache[i] = comp;
                    hits++;
                }
            }

            return hits;
        }

        public T AddGameComponent<T>() where T : ActorComponent {
            var comp = gameObject.AddComponent<T>();
            if (!Application.isPlaying) {
                // otherwise Awake() is called and that calls EnsureIntegrity
                comp.EnsureIntegrity();
            }

            return comp;
        }

        public World GetWorld() {
            return world;
        }

        public int GetRuntimeId() {
            return GetInstanceID();
        }

        public bool Is<T>() {
            return typeof(T).IsAssignableFrom(GetType());
        }
        
        public void Kill() {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                // First thing, get rid of the tick funcs to avoid shenanigans in the tickloop.
                // This ultimately happens before unity schedules OnDestroy calls.
                registeredComponents[i].UnregisterTickFunc();
            }
            Destroy(this);
        }

        protected virtual void DestroyActor() {
            if (world) {
                for (int i = 0; i < registeredComponents.Count; ++i) {
                    // Destroy components manually because they might be detached and will not be
                    // auto-removed when gameObject is destroyed.
                    var comp = registeredComponents[i];
                    Destroy(comp);
                }

                world.UnregisterTickFunc(actorTick);

                world.UnregisterActor(this);
            }

            Destroy(gameObject);
        }

        protected virtual void OnDestroy() {
            DestroyActor();
        }

        protected virtual void Reset() {
            // take the parent because otherwise this will always get this actor.
            // But we need the immediate next
            Actor parentActor = null;
            if (transform.parent) {
                parentActor = transform.parent.GetComponentInParent<Actor>();
            } 
            
            // Clear my registered components because Reset will re populate
            registeredComponents.Clear();
            if (parentActor && parentActor != this) {
                ParentActor = parentActor;
                // Resetting it will ensure integrity on all child components.
                // Child components will find this new actor as their new next closest actor in parents and attach themselves there instead.
                // Therefore we don't need to repeat this process here.
                parentActor.Reset();
            }
            else {
                ParentActor = null;
                // This is happening on root actors that have no parent.
                // get all components that may already exist because an actor was deleted and make them register themselves here.
                // This would also update components below this actor, if we added it to an existing actor
                var childs = GetComponentsInChildren<ActorComponent>(true);
                for (int i = 0; i < childs.Length; ++i) {
                    childs[i].EnsureIntegrity(true);
                }
            }
            
        }

        public void RegisterComponent(ActorComponent component) {
            if (registeredComponents.Contains(component)) {
                Debug.LogWarning($"Attempted to register component {component.GetType().Name} twice on actor {name}");
                return;
            }

            registeredComponents.Add(component);
            if (HasBegunPlaying) {
                world.ScheduleBeginPlay(component);
            }
        }

        public void UnregisterComponent(ActorComponent component) {
            registeredComponents.Remove(component);
            if (!component.IsBeingDestroyed) {
                Debug.LogWarning("Unregistered a component that wasn't being destroyed. Will destroy it now.");
                Destroy(component);
            }
        }

        public virtual void ProcessTick(float deltaTime, TickGroup tickGroup) {
            // there is no default behaviour
        }

        public bool HasComponent(ActorComponent component) {
            for (int i = 0; i < registeredComponents.Count; i++) {
                var comp = registeredComponents[i];
                if (comp == component) {
                    return true;
                }
            }

            return false;
        }
        
        public T GetGameMode<T>() where T : GameMode {
            return (T)GetWorld().GetGameMode();
        }
    }
}