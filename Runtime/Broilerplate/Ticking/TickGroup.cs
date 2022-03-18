namespace Broilerplate.Ticking {
    /// <summary>
    /// Group defines when a tick should happen.
    /// Mandated by Unity, we have 3 tick groups at our disposal.
    /// </summary>
    public enum TickGroup {
        Tick,
        LateTick,
        Physics
    }
}