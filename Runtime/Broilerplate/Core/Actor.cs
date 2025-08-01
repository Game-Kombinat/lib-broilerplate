﻿using System.Collections.Generic;
using System.Linq;
using Broilerplate.Core.Components;
using Broilerplate.Ticking;
using UnityEngine;
using ZLinq;
using Debug = UnityEngine.Debug;

namespace Broilerplate.Core {
    /// <summary>
    /// Base class for all gameplay relevant things that are in the world.
    /// Making use of an Actor will sort you into the life cycle of the broilerplate framework.
    /// You will use BeginPlay instead of Awake, LateBeginPlay instead of Start and ProcessTick() instead of any
    /// of the Unity update callbacks.
    /// <br/><br/>
    /// Awake can still be used (it's virtual) for logic that needs to happen even before BeginPlay.
    /// Define the tick loop in the TickFunc that comes with every actor.
    /// You can define one or more at once.
    /// <br/><br/>
    /// Actors can be hierarchical. Meaning you can have actors within actors.
    /// Parent actors can be Accessed via the ParentActor property.
    /// <see cref="ActorComponent"/>s will assume the closest parent actor as their owner automatically.
    /// <br/><br/>
    /// Actors are <see cref="IInitialise"/>, meaning they will receive BeginPlay and LateBeginPlay calls.
    /// </summary>
    [DisallowMultipleComponent]
    public class Actor : MonoBehaviour, ITickable, IThing, IInitialise {
        protected World world;


        /// <summary>
        /// TickFunc that is being registered with the world in this Actor LateBeginPlay.
        /// </summary>
        [SerializeField] protected TickFunc actorTick = new TickFunc();

        public int NumComponents => registeredComponents.Count;
        
        public Actor ParentActor { get; private set; }

        public bool HasTickFunc => actorTick != null;

        public int InitialisationPriority => actorTick?.Priority ?? 0;
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
            // register tick function to world.
            if (HasTickFunc && actorTick.CanEverTick) {
                actorTick.SetTickTarget(this);
                world.RegisterTickFunc(actorTick);
            }
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

        /// <summary>
        /// Retrieves the first-found actor component of type T on this Actor.
        /// Since actor components can be detached, a normal GetComponentInChildren call
        /// is not sufficient. That's why this exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetGameComponent<T>() {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                if (registeredComponents[i] is T t) {
                    return t;
                }
            }

            return default;
        }

        /// <summary>
        /// Same as GetGameComponent but for all of the components of given type.
        /// </summary>
        /// <param name="cache"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int GetGameComponents<T>(T[] cache) {
            int hits = 0;
            for (int i = 0; i < cache.Length; ++i) {
                if (registeredComponents.Count > i && registeredComponents[i] is T comp) {
                    cache[i] = comp;
                    hits++;
                }
            }

            return hits;
        }

        public List<T> GetGameComponents<T>() {
            return registeredComponents.AsValueEnumerable().OfType<T>().ToList();
        }

        /// <summary>
        /// Add an actor component of the given type dynamically.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Check if this actor is of the given type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Is<T>() {
            return typeof(T).IsAssignableFrom(GetType());
        }
        
        /// <summary>
        /// The proper way of destroying this actor.
        /// </summary>
        public void Kill() {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                // First thing, get rid of the tick funcs to avoid shenanigans in the tickloop.
                // This ultimately happens before unity schedules OnDestroy calls.
                registeredComponents[i].UnregisterTickFunc();
            }
            Destroy(this);
        }

        /// <summary>
        /// Encapsulates the logic of cleaning up this actor from the runtime data.
        /// Removes tickfuncs, destroys actor components and removes their tickfuncs as well.
        /// </summary>
        protected virtual void DestroyActor(bool callDestroy) {
            if (world) {
                for (int i = 0; i < registeredComponents.Count; ++i) {
                    // Destroy components manually because they might be detached and will not be
                    // auto-removed when gameObject is destroyed.
                    var comp = registeredComponents[i];
                    Destroy(comp);
                }
                world.UnregisterActor(this);
            }

            if (callDestroy) {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy() {
            DestroyActor(false);
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
            // this scenario could include components that are not actually in the actor hierarchy.
            // But since that isn't a hard requirement either way, we must make sure the actor is assigned manually.
            // calling EnsureIntegrity would be needless and too expensive as well.
            component.OnOwnerActorChanged(this);
            // if actor has begun playing, the original components are already registered, so we do it manually here.
            // otherwise we must wait it out for the actors BeginPlay
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
            return GetWorld().GetGameMode<T>();
        }
    }
}