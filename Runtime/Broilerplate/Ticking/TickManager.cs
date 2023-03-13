using System.Collections.Generic;
using Broilerplate.Core;
using UnityEngine;

namespace Broilerplate.Ticking {
    public class TickManager {
        private readonly World world;
        private readonly TickList ticks = new TickList();
        private readonly TickList physicsTicks = new TickList();
        private readonly TickList lateTicks = new TickList();
        private readonly List<TickFunc> scheduledAdds = new List<TickFunc>();
        private readonly List<TickFunc> scheduledRemovals = new List<TickFunc>();

        private readonly Queue<IInitialise> scheduledLateBeginPlays = new Queue<IInitialise>();

        public bool IsPaused { get; private set; }

        private float unityTimescaleAtPause;
        
        public void Pause(bool stopUnityTime = false) {
            IsPaused = true;
            if (stopUnityTime) {
                unityTimescaleAtPause = Time.timeScale;
                Time.timeScale = 0;
            }
        }

        public void Resume() {
            IsPaused = false;
            if (unityTimescaleAtPause > 0) {
                Time.timeScale = unityTimescaleAtPause;
                unityTimescaleAtPause = 0;
            }
        }

        public TickManager(World world) {
            this.world = world;
        }

        public virtual void Tick() {
            if (IsPaused) {
                return;
            }
            world.OnNewTick();
            ticks.Tick(world.timeData.deltaTime, world.timeData.timeSinceWorldBooted, TickGroup.Tick);
        }

        public virtual void PhysicsTick() {
            if (IsPaused) {
                return;
            }
            physicsTicks.Tick(world.timeData.deltaTime, world.timeData.timeSinceWorldBooted, TickGroup.Physics);
        }

        public virtual void LateTick() {
            if (IsPaused) {
                return;
            }
            lateTicks.Tick(world.timeData.deltaTime, world.timeData.timeSinceWorldBooted, TickGroup.LateTick);
            HandleScheduledLateBeginPlays();
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
                if (tickFunc.IsInTickGroup(TickGroup.Tick)) {
                    ticks.Remove(tickFunc);
                }

                if (tickFunc.IsInTickGroup(TickGroup.LateTick)) {
                    lateTicks.Remove(tickFunc);
                }

                if (tickFunc.IsInTickGroup(TickGroup.Physics)) {
                    physicsTicks.Remove(tickFunc);
                }
            }
            
            scheduledRemovals.Clear();
        }

        public void ScheduleBeginPlay(IInitialise i) {
            if (!i.HasBegunPlaying) {
                i.BeginPlay();
            }
            
            scheduledLateBeginPlays.Enqueue(i);
        }

        public void HandleScheduledLateBeginPlays() {
            // order is important, this is why this is a queue.
            // we want fi-fo. We get fi-fo.
            while (scheduledLateBeginPlays.Count > 0) {
                var begin = scheduledLateBeginPlays.Dequeue();
                if (!begin.HadLateBeginPlay) {
                    begin.LateBeginPlay();
                }
            }
        }
        
        public void HandleScheduledTickAdds() {
            bool sortTicks = false;
            bool sortLateTicks = false;
            bool sortPhysTicks = false;
            for (int i = 0; i < scheduledAdds.Count; ++i) {
                var tickFunc = scheduledAdds[i];
                
                if (tickFunc.IsInTickGroup(TickGroup.Tick)) {
                    ticks.Add(tickFunc);
                    sortTicks = true;
                }

                if (tickFunc.IsInTickGroup(TickGroup.LateTick)) {
                    lateTicks.Add(tickFunc);
                    sortLateTicks = true;
                }

                if (tickFunc.IsInTickGroup(TickGroup.Physics)) {
                    physicsTicks.Add(tickFunc);
                    sortPhysTicks = true;
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
    }
}