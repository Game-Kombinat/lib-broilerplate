using System;
using Broilerplate.Ticking;
using NaughtyAttributes;
using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// This is a special MonoBehaviour that requires an Actor in its parent or immediate hierarchy to attach to.
    /// It's guaranteed to get its IInitialise bound callbacks after its actor respectively.
    /// It can be hierarchically detached from its actor during runtime and remain available to the actor regardless.
    /// 
    /// </summary>
    public class ActorComponent : MonoBehaviour, ITickable, IThing, IInitialise {
        [SerializeField]
        protected Actor owner;
        
        [SerializeField]
        protected TickFunc componentTick = new TickFunc();

        private World world;

        public TickFunc ComponentTick => componentTick;

        public Actor Owner => owner;

        public bool IsBeingDestroyed { get; private set; } = false;

        public int InitialisationPriority => componentTick?.Priority ?? 0;
        public bool HasBegunPlaying { get; private set; } = false;
        
        public bool HadLateBeginPlay { get; private set; } = false;

        /// <summary>
        /// If true, detaches this component from its parent actor in the transform hierarchy.
        /// It will remain in the actors component register and can be gotten via
        /// GetGameComponent on the actor.
        /// Likewise this will be destroyed when the parent actor is destroyed.
        /// No matter if attached or not.
        ///
        /// Beware that this has no effect when the component is on the same GameObject as the Owner.
        /// </summary>
        [SerializeField]
        protected bool detachAtRuntime;

        [SerializeField, ShowIf(nameof(detachAtRuntime))]
        protected bool keepPosition = true;

        protected virtual void Awake() {
            // Have to have this for components added during runtime.
            // Editor-time authored components will be handled from actors Awake()
            EnsureIntegrity(true);
        }

        /// <summary>
        /// Called with the BeginPlay call on the actor this component is attached to.
        /// It's guaranteed to be called after the actors BeginPlay
        /// See <see cref="Actor.BeginPlay"/>
        /// </summary>
        public virtual void BeginPlay() {
            HasBegunPlaying = true;
            world = owner.GetWorld();
        }

        /// <summary>
        /// Stage 2 BeginPlay. This is called at the end of the frame the actor for this component
        /// was spawned in. It's guaranteed to be called after the actors LateBeginPlay.
        /// </summary>
        public virtual void LateBeginPlay() {
            HadLateBeginPlay = true;
            componentTick.SetTickTarget(this);
            GetWorld().RegisterTickFunc(componentTick);
            if (detachAtRuntime) {
                DetachFromActor();
            }
        }
        
        public virtual void ProcessTick(float deltaTime, TickGroup tickGroup) {
            // there is no default behaviour
        }
        
        protected virtual void Decommission() {
            UnregisterTickFunc();
            IsBeingDestroyed = true;
            // when called from OnDestroy this will become a null pointer before the deletion can be processed.
            // But it's okay, we don't dereference it. And all it does is destroying them anyway.
            // Would be a double-destroy
            owner.UnregisterComponent(this);
            // If we're detached and are being decommissioned we clear out everything
            if (detachAtRuntime) {
                Destroy(gameObject);
            }
        }


        protected virtual void OnDestroy() {
            
            // when the world is already destroyed there is no use in trying to do anything here.
            if (GetWorld() != null) {
                Decommission();
            }
        }

        protected virtual void Reset() {
            EnsureIntegrity(false, true);
        }
        
        public void SetEnableTick(bool shouldTick) {
            if (!componentTick.CanEverTick) {
                Debug.LogWarning($"Attempted to change tick on component that never ticks: {owner.name}.{name}");
                return;
            }
            
            componentTick.SetEnableTick(shouldTick);
        }

        public virtual void OnGameObjectStatusChange(bool isActive) {
            
        }

        public virtual void EnsureIntegrity(bool autoRegister = false, bool ignoreMissingActor = false) {
            var actor = transform.gameObject.GetComponentInParent<Actor>(true);
            if (!actor) {
                if (ignoreMissingActor) {
                    Debug.LogWarning("You added a GameComponent to a GameObject without an Actor in the parent hierarchy. Beware!");
                    return;
                }
                if (!Application.isPlaying) {
                    DestroyImmediate(this);
                }
                else {
                    Destroy(this);
                }
                throw new InvalidOperationException($"Component {GetType().Name} requires an Actor component on the same or a parent object!");
            }
            
            if (!owner || owner != actor) {
                owner = actor;
                OnOwnerActorChanged(owner);
            }
            
            if (autoRegister && !owner.HasComponent(this)) {
                owner.RegisterComponent(this);
            }
        }

        public World GetWorld() {
            return world;
        }

        public int GetRuntimeId() {
            return GetInstanceID();
        }

        public void UnregisterTickFunc() {
            var world = GetWorld();
            if (world) {
                world.UnregisterTickFunc(componentTick);
            }
        }

        public virtual void OnEnableTick() {
            
        }

        public virtual void OnDisableTick() {
            
        }

        /// <summary>
        /// This will break this actor component out of its actors game object hierarchy
        /// but will retain the connection to its actor. And the actor will keep this component
        /// in its component list regardless.
        /// </summary>
        public void DetachFromActor() {
            if (transform != Owner.transform) {
                transform.SetParent(null, keepPosition);
            }
        }
        
        // Change of owning actor if an actor has been injected into the hierarchy after the fact.
        public virtual void OnOwnerActorChanged(Actor newActor) {
            // owner = newActor;
        }
        
        public T GetGameMode<T>() where T : GameMode {
            return Owner.GetGameMode<T>();
        }
    }
}