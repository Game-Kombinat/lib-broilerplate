using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// This is a component for which there can be only one per game object
    /// since they rely on transforms
    /// </summary>
    [DisallowMultipleComponent] // this attribute actually suffices to enforce the above mentioned rule, also with inherited types
    public class ActorComponent : GameComponent {
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

        public override void BeginPlay() {
            base.BeginPlay();
            if (detachAtRuntime) {
                DetachFromActor();
            }
        }

        /// <summary>
        /// This will break this scene component out of its actors game object hierarchy
        /// but will retain the connection to its actor. And the actor will keep this component
        /// in its component list regardless.
        /// </summary>
        public void DetachFromActor() {
            if (transform != Owner.transform) {
                transform.SetParent(null, true);
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (owner && transform == owner.transform) {
                // This is a scene component on the root of a prefab which has also the actor on it.
                // we don't need to deal with this. This is the only case where scene components are
                // not in charge of their own gameobject
                return;
            }
            // note: this will invoke Destroy() on all other components that may exist on this GO
            // and they will clean up their registrations etc automatically, see GameComponent::OnDestroy
            // Generally, if the framework is used according to design, this cannot actually happen.
            // But in case it would, we got it covered
            Destroy(gameObject);
        }
    }
}