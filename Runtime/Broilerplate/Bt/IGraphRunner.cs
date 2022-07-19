using System.Collections;

namespace Broilerplate.Bt {
    public interface IGraphRunner {
        void TriggerExecution();
        void StopExecution();
        void PauseExecution();
        void ResumeExecution();
    }
}