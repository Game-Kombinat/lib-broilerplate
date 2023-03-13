namespace Broilerplate.Core {
    public interface IInitialise {
        bool HasBegunPlaying { get; }
        bool HadLateBeginPlay { get; }
        
        void BeginPlay();

        void LateBeginPlay();
    }
}