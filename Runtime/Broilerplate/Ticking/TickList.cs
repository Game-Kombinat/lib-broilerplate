using System.Collections.Generic;

namespace Broilerplate.Ticking {
    public class TickList {
        public readonly List<TickFunc> subjects = new List<TickFunc>();

        public void Add(TickFunc tickFunc) {
            subjects.Add(tickFunc);
        }

        public void Sort() {
            subjects.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public void Tick(float deltaTime, float timeSinceWorldBoot, TickGroup currentGroup) {
            for (int i = 0; i < subjects.Count; ++i) {
                var tick = subjects[i];
                if (tick.CanTickNow(timeSinceWorldBoot)) {
                    tick.Tick(deltaTime, timeSinceWorldBoot, currentGroup);
                }
            }
        }

        public void Remove(TickFunc tickFunc) {
            subjects.Remove(tickFunc);
        }
    }
}