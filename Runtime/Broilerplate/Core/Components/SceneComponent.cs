using UnityEngine;

namespace Broilerplate.Core.Components {
    /// <summary>
    /// This is a component for which there can be only one per game object
    /// since they rely on transforms
    /// </summary>
    [DisallowMultipleComponent] // this attribute actually suffices to enforce the above mentioned rule, also with inherited types
    public class SceneComponent : GameComponent {
        
    }
}