using System;
using UnityEngine;

namespace Broilerplate.Tools {
    public static class LevelLoadProgress {
        public static Action<float> yourActions;

        public static void OnProgress(float t) {
            yourActions?.Invoke(Mathf.Clamp01(t));
        }

        public static void Clear() {
            yourActions = null;
        }
    }
}