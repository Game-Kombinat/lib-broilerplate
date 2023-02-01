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
        Physics = 1 << 2
    }
}