using System;
using System.Collections.Generic;
using Broilerplate.Core;

namespace Broilerplate.Ticking {
    public class TickManager {
        private readonly World world;
        private readonly TickList ticks = new TickList();
        private readonly TickList physicsTicks = new TickList();
        private readonly TickList lateTicks = new TickList();
        private readonly List<TickFunc> scheduledAdds = new List<TickFunc>();
        private readonly List<TickFunc> scheduledRemovals = new List<TickFunc>();

        public TickManager(World world) {
            this.world = world;
        }

        public virtual void Tick() {
            world.OnNewTick();
            ticks.Tick(world.timeData.deltaTime, world.timeData.timeSinceWorldBooted);
        }

        public virtual void PhysicsTick() {
            physicsTicks.Tick(world.timeData.deltaTime, world.timeData.timeSinceWorldBooted);
        }

        public virtual void LateTick() {
            lateTicks.Tick(world.timeData.deltaTime, world.timeData.timeSinceWorldBooted);
        }

        public void RegisterTickFunc(TickFunc tickFunc) {
            tickFunc.OnReset();
            scheduledAdds.Add(tickFunc);
        }

        public void UnregisterTickFunc(TickFunc tickFunc) {
            scheduledRemovals.Add(tickFunc);
        }

        public void HandleScheduledTickRemovals() {
            for (int i = 0; i < scheduledRemovals.Count; ++i) {
                var tickFunc = scheduledRemovals[i];
                switch (tickFunc.TickGroup) {
                    case TickGroup.Tick:
                        ticks.Remove(tickFunc);
                        break;
                    case TickGroup.LateTick:
                        lateTicks.Remove(tickFunc);
                        break;
                    case TickGroup.Physics:
                        physicsTicks.Remove(tickFunc);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unknown tick group: {tickFunc.TickGroup}");
                }
            }
            
            scheduledRemovals.Clear();
        }
        
        public void HandleScheduledTickAdds() {
            bool sortTicks = false;
            bool sortLateTicks = false;
            bool sortPhysTicks = false;
            for (int i = 0; i < scheduledAdds.Count; ++i) {
                var tickFunc = scheduledAdds[i];
                switch (tickFunc.TickGroup) {
                    case TickGroup.Tick:
                        ticks.Add(tickFunc);
                        sortTicks = true;
                        break;
                    case TickGroup.LateTick:
                        lateTicks.Add(tickFunc);
                        sortLateTicks = true;
                        break;
                    case TickGroup.Physics:
                        physicsTicks.Add(tickFunc);
                        sortPhysTicks = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unknown tick group: {tickFunc.TickGroup}");
                }
            }

            scheduledAdds.Clear();

            if (sortTicks) {
                ticks.Sort();
            }

            if (sortLateTicks) {
                lateTicks.Sort();
            }

            if (sortPhysTicks) {
                physicsTicks.Sort();
            }
        }

        private TickList GetListForTickGroup(TickGroup group) {
            switch (group) {
                case TickGroup.Tick:
                    return ticks;
                case TickGroup.LateTick:
                    return lateTicks;
                case TickGroup.Physics:
                    return physicsTicks;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@group), @group, null);
            }
        }
    }
}