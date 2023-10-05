namespace Broilerplate.Core {
    /// <summary>
    /// Provides a contract for the BeginPlay routines.
    /// Everything that should have them should implement this interface.
    /// </summary>
    public interface IInitialise {
        /// <summary>
        /// Initialisation priority can override default initialisation order to
        /// resolve race conditions and other init order issues.
        /// Notably: This affects only LateBeginPlay because BeginPlay will always be immediate.
        /// Values &lt; 0 are executed earlier and values &gt; are executed later.
        /// </summary>
        int InitialisationPriority { get; }
        bool HasBegunPlaying { get; }
        bool HadLateBeginPlay { get; }
        
        /// <summary>
        /// Immediately called after the IInitialise was created and registered with the world.
        /// </summary>
        void BeginPlay();

        /// <summary>
        /// Called at the end of LateUpdate.
        /// The order of the calls can be determined by setting the InitialisationPriority.
        /// By default, this will act like a fi-fo queue.
        /// </summary>
        void LateBeginPlay();
    }
}