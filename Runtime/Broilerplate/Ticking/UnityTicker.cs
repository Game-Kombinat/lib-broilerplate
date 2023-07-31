using UnityEngine;

namespace Broilerplate.Ticking {
    /// <summary>
    /// This binds the broiler tick system to unity's tick loops.
    /// It's automatically being instantiated as part of bootstrapping the world.
    /// This is created right after the world has been instantiated.
    /// todo: Implement manual physics stepping. Then we can split physics callbacks into pre and post physics step.
    /// </summary>
    public class UnityTicker : MonoBehaviour {
        private TickManager tickManager;

        public void SetTickManager(TickManager tm) {
            tickManager = tm;
        }

        private void Update() {
            // tick manager can become null for reasons that are currently not known.
            // Possibly when the host hardware had suspended the application and is un-suspending it.
            // Seems unlikely though.
            if (tickManager == null) {
                Debug.LogError("TickManager has become null. Disabling ticking");
                enabled = false;
                return;
            }
            
            tickManager?.Tick();
        }

        private void LateUpdate() {
            tickManager?.LateTick();
        }

        private void FixedUpdate() {
            tickManager?.PhysicsTick();
        }
    }
}