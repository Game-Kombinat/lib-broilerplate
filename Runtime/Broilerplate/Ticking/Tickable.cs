namespace Broilerplate.Ticking {
    public interface ITickable {
        
        void ProcessTick(float deltaTime);
        void SetEnableTick(bool shouldTick);
        void UnregisterTickFunc();
    }
}