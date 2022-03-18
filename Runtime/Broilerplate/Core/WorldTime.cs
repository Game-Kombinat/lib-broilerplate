using System;

namespace Broilerplate.Core {
    public struct WorldTime {
        public DateTime lastTick;
        public DateTime thisTick;
        
        public float deltaTime;
        public float timeSinceWorldBooted;

        public float timeDilation;
    }
}