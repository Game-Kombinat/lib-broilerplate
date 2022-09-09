using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// This actor component requires an actor somewhere in its immediate parent hierarchy.
    /// Its lifecycle is controlled by the actors lifecycle, giving you exact knowledge
    /// when and how this component will be initialised and destroyed.
    /// </summary>
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
    }
}