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

        public void Reset() {
            var actor = transform.root.gameObject.GetComponent<Actor>();
            if (!actor) {
                Debug.LogError($"Component {GetType().Name} requires an Actor component on the root object!");
                DestroyImmediate(this);
                return;
            }

            owner = actor;
            // unity GetComponent causes a native call, we want to avoid them where possible.
            // so we keep a managed-land component list.
            owner.RegisterComponent(this);
        }

        public World GetWorld() {
            return owner.GetWorld();
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
            owner.UnregisterComponent(this);
        }

        // todo: this happens ofc when components are destroyed on play end.
        // and then these messages are not appropriate. This needs to be circumvented somehow.
        protected virtual void OnDestroy() {
            // when the world is already destroyed there is no use in trying to do anything here.
            if (!PendingDestruction && GetWorld() != null) {
                // someone called Destroy() directly. But it also happens when playmode stops
                // so we have to deal with this gracefully rather than chopping it all down.
                Decommission();
            }
        }
    }
}