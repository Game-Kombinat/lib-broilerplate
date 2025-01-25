using UnityEngine;

namespace Broilerplate.Core.Subsystems {
    /// <summary>
    /// Just a base class in case I'll make more sub systems.
    /// Represents a subsystem. Depending on implementation they exist in different places and for different lifecycles.
    /// </summary>
    public abstract class SubsystemBase : ScriptableObject, IInitialise {
        [SerializeField]
        private int initPriority;

        public int InitialisationPriority => initPriority;
        
        public bool HasBegunPlaying { get; private set; } = false;
        public bool HadLateBeginPlay { get; private set; } = false;

        public virtual void BeginPlay() {
            HasBegunPlaying = true;
        }

        public virtual void LateBeginPlay() {
            HadLateBeginPlay = true;
        }

        public virtual bool ShouldLoad() => true;
    }
}