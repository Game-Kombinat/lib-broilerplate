using System;
using UnityEngine;

namespace Broilerplate.Ticking {
    public class UnityTicker : MonoBehaviour {
        [SerializeField]
        private TickManager tickManager;

        public void SetTickManager(TickManager tm) {
            tickManager = tm;
        }

        private void Update() {
            tickManager.Tick();
        }

        private void LateUpdate() {
            tickManager.LateTick();
        }

        private void FixedUpdate() {
            tickManager.PhysicsTick();
        }
    }
}