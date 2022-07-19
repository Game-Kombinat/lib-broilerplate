using System.Collections;

namespace GameKombinat.ControlFlow.Bt {
    public interface IGraphRunner {
        void TriggerExecution();
        void StopExecution();
        void PauseExecution();
        void ResumeExecution();
        IEnumerator Tick();
    }
}