using System.Collections.Generic;
using Broilerplate.Core;
using UnityEngine;

namespace Broilerplate.Ticking {
    /// <summary>
    /// Provides an API into broilerplates tickable system.
    /// This is driven by Unitys update loops in the <see cref="TickManager"/>
    /// </summary>
    public class TickManager {
        /// <summary>
        /// The world this tickmanager is ticking for
        /// </summary>
        private readonly World world;
        
        /// <summary>
        /// List of tickables for the pre-physics update loop
        /// </summary>
        private readonly TickList ticks = new TickList();
        
        /// <summary>
        /// List of tickables for the during-physics update loop
        /// </summary>
        private readonly TickList physicsTicks = new TickList();
        
        /// <summary>
        /// List of tickables for the late-tick (post-physics) update loop
        /// </summary>
        private readonly TickList lateTicks = new TickList();
        
        /// <summary>
        /// List of tickables that want to be added for the next frame's tick
        /// </summary>
        private readonly List<TickFunc> scheduledAdds = new List<TickFunc>();
        
        /// <summary>
        /// List of tickables that need to be removed before the next frame's tick
        /// </summary>
        private readonly List<TickFunc> scheduledRemovals = new List<TickFunc>();

        /// <summary>
        /// List of IInitialise interfaces that are awaiting their LateBeginPlay call.
        /// </summary>
        private readonly Queue<IInitialise> scheduledLateBeginPlays = new Queue<IInitialise>();

        public bool IsPaused { get; private set; }

        private float timeScaleAtPause;
        
        public void Pause(bool zeroTimescale = false) {
            IsPaused = true;
            if (zeroTimescale) {
                timeScaleAtPause = Time.timeScale;
                Time.timeScale = 0;
            }
        }

        public void Resume() {
            IsPaused = false;
            if (timeScaleAtPause > 0) {
                Time.timeScale = timeScaleAtPause;
                timeScaleAtPause = 0;
            }
        }

        public TickManager(World world) {
            this.world = world;
        }

        public virtual void Tick() {
            if (IsPaused) {
                return;
            }
            world.HandleTickChanges();
            ticks.Tick(Time.deltaTime, Time.timeSinceLevelLoad, TickGroup.Tick);
        }

        public virtual void PhysicsTick() {
            if (IsPaused) {
                return;
            }
            physicsTicks.Tick(Time.fixedDeltaTime, Time.timeSinceLevelLoad, TickGroup.Physics);
        }

        public virtual void LateTick() {
            if (IsPaused) {
                return;
            }
            lateTicks.Tick(Time.deltaTime, Time.timeSinceLevelLoad, TickGroup.LateTick);
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