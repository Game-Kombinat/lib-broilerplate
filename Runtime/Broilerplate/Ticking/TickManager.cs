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
        private readonly TickList ticks;
        
        /// <summary>
        /// List of tickables for the during-physics update loop
        /// </summary>
        private readonly TickList physicsTicks;
        
        /// <summary>
        /// List of tickables for the late-tick (post-physics) update loop
        /// </summary>
        private readonly TickList lateTicks;
        
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
        private readonly List<IInitialise> scheduledLateBeginPlays = new ();

        public bool IsPaused { get; private set; }

        private float timeScaleAtPause;
        
        public TickManager(World world) {
            this.world = world;
            ticks = new TickList(this);
            physicsTicks = new TickList(this);
            lateTicks = new TickList(this);
        }

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

        public virtual void Tick() {
            world.HandleTickChanges();
            ticks.Tick(Time.deltaTime, Time.timeSinceLevelLoad, TickGroup.Tick);
        }

        public virtual void PhysicsTick() {
            world.HandleTickChanges();
            physicsTicks.Tick(Time.fixedDeltaTime, Time.timeSinceLevelLoad, TickGroup.Physics);
        }

        public virtual void LateTick() {
            lateTicks.Tick(Time.deltaTime, Time.timeSinceLevelLoad, TickGroup.LateTick);
            HandleScheduledLateBeginPlays();
        }

        public void RegisterTickFunc(TickFunc tickFunc) {
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

                tickFunc.MarkUnregistered();
            }
            
            scheduledRemovals.Clear();
        }

        public void ScheduleBeginPlay(IInitialise i) {
            if (!i.HasBegunPlaying) {
                i.BeginPlay();
            }
            
            scheduledLateBeginPlays.Add(i);
        }

        public void HandleScheduledLateBeginPlays() {
            if (scheduledLateBeginPlays.Count == 0) {
                return;
            }
            
            // see if we need to re-order the list elements if they demand higher sort order.
            // This is a mechanic to resolve initialisation order issues.
            scheduledLateBeginPlays.Sort((a, b) => {
                return a == null || b == null
                    ? 0
                    : a.InitialisationPriority.CompareTo(b.InitialisationPriority);
            });

            // since we start at zero, assuming nobody has overridden the initialisation priority, we get fi-fo behaviour.
            for (int i = 0; i < scheduledLateBeginPlays.Count; i++) {
                var begin = scheduledLateBeginPlays[i];
                if (begin == null) {
                    // This happens when we spawn an actor and destroy it right away in the same frame.
                    // Which we just might.
                    continue;
                }
                if (!begin.HadLateBeginPlay) {
                    begin.LateBeginPlay();
                }
            }
            
            scheduledLateBeginPlays.Clear();
        }
        
        public void HandleScheduledTickAdds() {
            bool sortTicks = false;
            bool sortLateTicks = false;
            bool sortPhysTicks = false;
            for (int i = 0; i < scheduledAdds.Count; ++i) {
                var tickFunc = scheduledAdds[i];
                if (tickFunc.IsRegistered) {
                    Debug.LogWarning("TickFunc already registered.");
                    continue;
                }
                tickFunc.OnReset();
                
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

                tickFunc.MarkRegistered();
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