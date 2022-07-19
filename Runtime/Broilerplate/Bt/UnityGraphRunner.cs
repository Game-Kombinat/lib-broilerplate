using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace GameKombinat.ControlFlow.Bt {
    /// <summary>
    /// This behaviour tree runner will cache the active children in a list and tick them through.
    /// This effectively means the tree will not be traversed on each frame.
    /// Instead, the active children are ticked without pre conditions being checked.
    /// However, parents remain in the tick list until their children are done executing so
    /// there is a measure of control in the way sequencers and such operate.
    /// So we safe some traversal performance at the cost of immediate control.
    /// </summary>
    public class UnityGraphRunner : MonoBehaviour, IGraphRunner {
        [SerializeField]
        [ReadOnly]
        public BehaviourTree runnable;
        
        [SerializeField]
        public bool beginOnStart;

        private Coroutine tickingRoutine;

        private void Start() {
            if (beginOnStart) {
                runnable.Begin();
                if (tickingRoutine != null) {
                    StopCoroutine(tickingRoutine);
                }

                tickingRoutine = StartCoroutine(Tick());
            }
        }

        public void TriggerExecution() {
            if (!beginOnStart && !runnable.IsRunning) {
                runnable.Begin();
                if (tickingRoutine != null) {
                    StopCoroutine(tickingRoutine);
                }
                tickingRoutine = StartCoroutine(Tick());
            }
        }

        public void StopExecution() {
            if (tickingRoutine != null) {
                StopCoroutine(tickingRoutine);
            }
            runnable.Terminate();
        }

        public void PauseExecution() {
            runnable.Pause();
        }

        public void ResumeExecution() {
            runnable.Resume();
        }
        
        public IEnumerator Tick() {
            var status = runnable.Status;
            while (!(status == TaskStatus.Success || status == TaskStatus.Failure)) {
                status = runnable.Status;
                runnable.Tick();
                yield return null;
            }
            TerminateExecutorAndFinish();
        }

        private void TerminateExecutorAndFinish() {
            runnable.Terminate();
        }
    }
}