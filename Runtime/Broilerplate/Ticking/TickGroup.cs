using System;

namespace Broilerplate.Ticking {
    /// <summary>
    /// Group defines when a tick should happen.
    /// Mandated by Unity, we have 3 tick groups at our disposal.
    /// Any tickfunc can be called in multiple tick groups
    /// </summary>
    [Flags, Serializable]
    public enum TickGroup {
        None = 0,
        Tick = 1 << 0,
        LateTick = 1 << 1,
        Physics = 1 << 2,
        IgnorePause = 1 << 3 // special flag to make the tickfunc ignore if the game is paused.
    }

    public static class TickGroupExtensions {
        public static bool MatchesAny(this TickGroup g, TickGroup other) {
            return (g & other) != TickGroup.None;
        }
        
        public static bool Matches(this TickGroup g, TickGroup other) {
            return (g & other) == other;
        }
    }
}