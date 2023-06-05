using System;

namespace Broilerplate.Core {
    /// <summary>
    /// Represents a time structure that is tied to broilerplates tick timing rather than Unitys Time function.
    /// It's existence is a side effect of custom update loops with custom time dilation.
    /// todo: Ideally, we can eventually get rid of this and make use of Unitys time system like we should be.
    /// It's certainly possible.
    /// </summary>
    public struct WorldTime {
        /// <summary>
        /// Time the last tick started
        /// </summary>
        public DateTime lastTick;
        
        /// <summary>
        /// Time the current tick started
        /// </summary>
        public DateTime thisTick;
        
        /// <summary>
        /// Delta between last and current tick start
        /// </summary>
        public float deltaTime;
        
        /// <summary>
        /// TIme in seconds since the world booted up.
        /// </summary>
        public float timeSinceWorldBooted;

        /// <summary>
        /// Multiplier to the time for speed-up / slow-down purposes
        /// </summary>
        public float timeDilation;
    }
}