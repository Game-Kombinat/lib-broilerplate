using UnityEngine;

namespace Broilerplate.Core.Subsystems {
    /// <summary>
    /// Just a base class in case I'll make more sub systems.
    /// </summary>
    public abstract class SubsystemBase : ScriptableObject, IInitialise {
        public bool HasBegunPlaying { get; private set; } = false;
        public bool HadLateBeginPlay { get; private set; } = false;

        public virtual void BeginPlay() {
            HasBegunPlaying = true;
        }

        public virtual void LateBeginPlay() {
            HadLateBeginPlay = true;
        }
    }
}