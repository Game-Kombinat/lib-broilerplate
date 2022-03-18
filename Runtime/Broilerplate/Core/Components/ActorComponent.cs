using System;
using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// These are components that can be attached anywhere.
    /// That is because they do not require a transform to operate.
    /// </summary>
    public class ActorComponent : GameComponent {
        public override void EnsureIntegrity() {
            if (transform != transform.root) {
                DestroyImmediate(this);
                throw new InvalidOperationException("Actor Component must be added to the root, where the actor is located!");
            }
            base.EnsureIntegrity();
            
        }
    }
}