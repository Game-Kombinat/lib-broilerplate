namespace Broilerplate.Core {
    /// <summary>
    /// Provides a contract for the BeginPlay routines.
    /// Everything that should have them should implement this interface.
    /// </summary>
    public interface IInitialise {
        bool HasBegunPlaying { get; }
        bool HadLateBeginPlay { get; }
        
        void BeginPlay();

        void LateBeginPlay();
    }
}