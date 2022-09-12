using System;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// Component that explicitly ties to an actor.
    /// </summary>
    public class GameComponent : MonoBehaviour, ITickable, IThing {
        [SerializeField]
        protected Actor owner;
        
        [SerializeField]
        protected TickFunc componentTick = new TickFunc();

        public TickFunc ComponentTick => componentTick;

        public Actor Owner => owner;

        public bool IsBeingDestroyed { get; private set; } = false;
        
        public bool HasBegunPlaying { get; private set; } = false;

        public GameComponent() {
            componentTick.SetCanEverTick(true);
        }

        protected virtual void Awake() {
            // Have to have this for components added during runtime.
            // Editor-time authored components will be handled from actors Awake()
            EnsureIntegrity(true);
        }

        public virtual void BeginPlay() {
            HasBegunPlaying = true;
            componentTick.SetTickTarget(this);
            GetWorld().RegisterTickFunc(componentTick);
        }

        // Change of owning actor if an actor has been injected into the hierarchy after the fact.
        public virtual void OnOwnerActorChanged(Actor newActor) {
            // owner = newActor;
        }
        
        public void SetEnableTick(bool shouldTick) {
            if (!componentTick.CanEverTick) {
                Debug.LogWarning($"Attempted to change tick on component that never ticks: {owner.name}.{name}");
                return;
            }
            
            componentTick.SetEnableTick(shouldTick);
        }

        public virtual void EnsureIntegrity(bool autoRegister = false) {
            var actor = transform.gameObject.GetComponentInParent<Actor>();
            if (!actor) {
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
            return owner == null ? null : owner.GetWorld();
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

        public virtual void ProcessTick(float deltaTime) {
            // there is no default behaviour
        }

        protected virtual void Decommission() {
            UnregisterTickFunc();
            // when called from OnDestroy this will become a null pointer before the deletion can be processed.
            // But it's okay, we don't dereference it. And all it does is destroying them anyway.
            // Would be a double-destroy
            owner.UnregisterComponent(this);
        }


        protected virtual void OnDestroy() {
            IsBeingDestroyed = true;
            // when the world is already destroyed there is no use in trying to do anything here.
            if (GetWorld() != null) {
                Decommission();
            }
        }

        protected virtual void Reset() {
            EnsureIntegrity();
        }
    }
}