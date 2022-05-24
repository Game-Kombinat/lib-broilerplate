using System;

namespace Broilerplate.Core {
    public struct WorldTime {
        public DateTime lastTick;
        public DateTime thisTick;
        
        public float deltaTime;
        public float timeSinceWorldBooted;

        public float timeDilation;

        public WorldTime(object kek = null) { // fuck you c#. I want my parameterless constructors!
            lastTick = DateTime.Now;
            thisTick = lastTick;
            deltaTime = 0;
            timeSinceWorldBooted = 0;
            timeDilation = 1;
        }
    }
}