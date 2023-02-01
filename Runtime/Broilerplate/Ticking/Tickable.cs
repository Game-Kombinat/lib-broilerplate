namespace Broilerplate.Ticking {
    public interface ITickable {
        
        void ProcessTick(float deltaTime, TickGroup tickGroup);
        void SetEnableTick(bool shouldTick);
        void UnregisterTickFunc();

        void OnEnableTick();

        void OnDisableTick();
    }
}