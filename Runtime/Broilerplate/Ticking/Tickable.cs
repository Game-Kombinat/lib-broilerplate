namespace Broilerplate.Ticking {
    
    /// <summary>
    /// Interface to provide methods for things that want to be ticked.
    /// </summary>
    public interface ITickable {
        
        void ProcessTick(float deltaTime, TickGroup tickGroup);
        void SetEnableTick(bool shouldTick);
        void UnregisterTickFunc();

        void OnEnableTick();

        void OnDisableTick();
    }
}