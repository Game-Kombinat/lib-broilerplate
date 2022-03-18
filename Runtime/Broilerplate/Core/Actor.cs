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
    public class Actor : MonoBehaviour, ITickable {

        protected World world;

        
        [SerializeField]
        protected TickFunc actorTick = new TickFunc();

        public int NumComponents => registeredComponents.Count;

        public bool HasTickFunc => actorTick != null;

        [SerializeField]
        private List<GameComponent> registeredComponents = new List<GameComponent>();
        private readonly List<GameComponent> componentRemovalSchedule = new List<GameComponent>();
        

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
            if (!HasTickFunc || !actorTick.CanEverTick) {
                return;
            }
            actorTick.SetTickTarget(this);

            world.RegisterTickFunc(actorTick);

            // why do this? So we know the owner actor has been initialised with BeginPlay before its components.
            // Now we get a known execution order.
            for (int i = 0; i < registeredComponents.Count; ++i) {
                registeredComponents[i].BeginPlay();
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

        public void RemoveGameComponent<T>(bool all = false) where T : GameComponent {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                if (registeredComponents[i] is T) {
                    registeredComponents[i].DestroyComponent();
                    if (!all) {
                        break;
                    }
                }
            }
        }

        public T AddGameComponent<T>() where T : GameComponent {
            if (typeof(T) == typeof(SceneComponent)) {
                var go = new GameObject($"{typeof(T).Name} Container");
                go.transform.SetParent(transform);
                var c = go.AddComponent<T>();
                c.Reset();
                return c;
            }

            var comp = gameObject.AddComponent<T>();
            comp.Reset();
            return comp;
        }

        public World GetWorld() {
            return world;
        }

        public virtual void DestroyActor() {
            for (int i = 0; i < registeredComponents.Count; ++i) {
                registeredComponents[i].DestroyComponent();
            }
            ProcessComponentRemoval();
            world.DestroyActor(this);
        }

        private void Reset() {
            if (transform.root != transform) {
                Debug.LogError($"Actor components must be added on the root component {transform.root.name} but this is {name}");
                DestroyImmediate(this);
            }
            
            // get all components that may already exist because an actor was deleted and make them register themselves here.
            var childs = GetComponentsInChildren<GameComponent>();
            for (int i = 0; i < childs.Length; ++i) {
                childs[i].Reset();
            }
        }
        

        // todo: problem here: this needs to be called at some point to actually remove components
        // and it would have to happen as soon as there is a component scheduled for removal.
        // we do this in the first place to not remove a component mid-frame. Ideally it happens after
        // the frame is over to avoid any issues when someone is looping the component list.
        public void ProcessComponentRemoval() {
            for (int i = 0; i < componentRemovalSchedule.Count; ++i) {
                if (registeredComponents.Remove(componentRemovalSchedule[i])) {
                    Destroy(componentRemovalSchedule[i]);
                }
                
            }
            componentRemovalSchedule.Clear();
        }

        public void RegisterComponent(GameComponent component) {
            registeredComponents.Add(component);
        }

        public void UnregisterComponent(GameComponent component) {
            if (world) {
                componentRemovalSchedule.Add(component);
                world.MarkDirty(this);
            }
        }

        public virtual void ProcessTick(float deltaTime) {
            // there is no default behaviour
        }
    }

    public static class ActorHelperExtensions {
        public static T SpawnActor<T>(this GameObject go, World world) where T : Actor {
            var actor = go.AddComponent<Actor>();
            actor.SetWorld(world);
            actor.BeginPlay();
            return (T)actor;
        }
    }
}