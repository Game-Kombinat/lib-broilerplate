using System;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// Component that explicitly ties to an actor.
    /// </summary>
    public class GameComponent : MonoBehaviour, ITickable {
        [SerializeField]
        protected Actor owner;
        
        [SerializeField]
        protected TickFunc componentTick = new TickFunc();

        public TickFunc ComponentTick => componentTick;

        public bool PendingDestruction { get; private set; }

        public GameComponent() {
            componentTick.SetCanEverTick(true);
        }

        private void Awake() {
            EnsureIntegrity();
        }

        public virtual void BeginPlay() {
            componentTick.SetTickTarget(this);
            GetWorld().RegisterTickFunc(componentTick);
        }
        
        public void SetEnableTick(bool shouldTick) {
            if (!componentTick.CanEverTick) {
                Debug.LogWarning($"Attempted to change tick on component that never ticks: {owner.name}.{name}");
                return;
            }
            
            componentTick.SetEnableTick(shouldTick);
        }

        public virtual void EnsureIntegrity() {
            Debug.Log($"{name} Reset!");
            var actor = transform.root.gameObject.GetComponent<Actor>();
            if (!actor) {
                DestroyImmediate(this);
                throw new InvalidOperationException($"Component {GetType().Name} requires an Actor component on the root object!");
            }

            owner = actor;
            // unity GetComponent causes a native call, we want to avoid them where possible.
            // so we keep a managed-land component list.
            owner.RegisterComponent(this);
        }

        public World GetWorld() {
            return owner == null ? null : owner.GetWorld();
        }

        public void UnregisterTickFunc() {
            var world = GetWorld();
            if (world) {
                world.UnregisterTickFunc(componentTick);
            }
        }

        public virtual void DestroyComponent() {
            Decommission();
        }

        public void ProcessTick(float deltaTime) {
            // there is no default behaviour
        }

        private void Decommission() {
            UnregisterTickFunc();
            PendingDestruction = true;
            // when called from OnDestroy this will become a null pointer before the deletion can be processed.
            // But it's okay, we don't dereference it. And all it does is destroying them anyway.
            // Would be a double-destroy
            owner.UnregisterComponent(this);
        }


        protected virtual void OnDestroy() {
            // when the world is already destroyed there is no use in trying to do anything here.
            if (!PendingDestruction && GetWorld() != null) {
                // This happens when
                // a) game ends
                // b) someone calls Destroy() directly
                // c) There were other actor components on the same GO that was bound to a scene component which is destroyed
                // And in either case we need to make sure that they all don't produce any null pointers
                Decommission();
            }
        }

        private void Reset() {
            EnsureIntegrity();
        }
    }
}