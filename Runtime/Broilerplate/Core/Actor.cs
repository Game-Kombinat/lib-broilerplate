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
    public class Actor : MonoBehaviour, ITickable, IThing {
        protected World world;


        [SerializeField] protected TickFunc actorTick = new TickFunc();

        public int NumComponents => registeredComponents.Count;

        public bool HasTickFunc => actorTick != null;

        public bool HasBegunPlaying { get; private set; } = false;

        private readonly List<GameComponent> registeredComponents = new List<GameComponent>();


        public TickFunc ActorTick => actorTick;

        public Actor() {
            actorTick.SetCanEverTick(true);
        }


        /// <summary>
        /// Called when this actor is spawned.
        /// This function replaces Start() with a managed function.
        /// Also works on actors already in the scene.
        /// Be sure to call the super function to ensure everything is working!
        /// </summary>
        public virtual void BeginPlay() {
            // register tick function to world.
            if (HasTickFunc && actorTick.CanEverTick) {
                actorTick.SetTickTarget(this);
                world.RegisterTickFunc(actorTick);;
            }

            var attachedComps = GetComponentsInChildren<GameComponent>();

            // why do this? So we know the owner actor has been initialised with BeginPlay before its components.
            // Now we get a known execution order.
            for (int i = 0; i < attachedComps.Length; ++i) {
                var comp = attachedComps[i];
                comp.BeginPlay();
            }

            // mark as ready so when components get added now, during runtime,
            // they get BeginPlay called right away.
            HasBegunPlaying = true;
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

        /// <summary>
        /// Called before BeginPlay either when actor is spawned or fetched from existing world.
        /// </summary>
        /// <param name="inWorld"></param>
        public void SetWorld(World inWorld) {
            world = inWorld;
        }

        public T GetGameComponent<T>() where T : GameComponent {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                if (registeredComponents[i] is T) {
                    return (T)registeredComponents[i];
                }
            }

            return null;
        }

        public T AddGameComponent<T>() where T : GameComponent {
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

        protected virtual void DestroyActor() {
            if (world) {
                for (int i = 0; i < registeredComponents.Count; ++i) {
                    // Destroy components that are part of the actor but not part of the game object hierarchy.
                    // These are detached scene components. And they would not be caught when destroying the game object
                    // of this actor
                    var comp = registeredComponents[i];
                    if (comp.transform.root != transform) {
                        Destroy(comp);
                    }
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
            if (transform.root != transform) {
                Debug.LogError(
                    $"Actor components must be added on the root component {transform.root.name} but this is {name}");
                DestroyImmediate(this);
            }

            // get all components that may already exist because an actor was deleted and make them register themselves here.
            var childs = GetComponentsInChildren<GameComponent>();
            for (int i = 0; i < childs.Length; ++i) {
                childs[i].EnsureIntegrity();
            }
        }

        public void RegisterComponent(GameComponent component) {
            if (registeredComponents.Contains(component)) {
                Debug.LogWarning($"Attempted to register component {component.GetType().Name} twice on actor {name}");
                return;
            }

            registeredComponents.Add(component);
            if (HasBegunPlaying) {
                component.BeginPlay();
            }
        }

        public void UnregisterComponent(GameComponent component) {
            registeredComponents.Remove(component);
            if (!component.IsBeingDestroyed) {
                Debug.LogWarning("Unregistered a component that wasn't being destroyed. Will destroy it now.");
                Destroy(component);
            }
        }

        public virtual void ProcessTick(float deltaTime) {
            // there is no default behaviour
        }
    }

    public static class ActorHelperExtensions {
        // todo: this is not test covered, test cover it. Doesn't look like this would work atm
        public static T SpawnActor<T>(this GameObject go, World world) where T : Actor {
            var actor = go.AddComponent<Actor>();
            actor.SetWorld(world);
            actor.BeginPlay();
            return (T)actor;
        }
    }
}